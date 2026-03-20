/**
 * WebRTC Mesh implementation for Flux Huddles
 */
const peerConnections = {}; // targetUserId -> RTCPeerConnection
let localStream = null;
let screenStream = null;
let dotNetHelper = null;
let audioContext = null;
let analyser = null;
let microphone = null;
let javascriptNode = null;

const configuration = {
    iceServers: [
        { urls: 'stun:stun.l.google.com:19302' },
        { urls: 'stun:stun1.l.google.com:19302' }
    ]
};

window.webrtc = {
    initialize: async (helper) => {
        dotNetHelper = helper;
    },

    getLocalStream: async (requestAudio, requestVideo) => {
        try {
            if (localStream) {
                localStream.getTracks().forEach(track => track.stop());
            }

            // Check if devices exist before requesting them
            const devices = await navigator.mediaDevices.enumerateDevices();
            const hasVideoDevice = devices.some(device => device.kind === 'videoinput');
            const hasAudioDevice = devices.some(device => device.kind === 'audioinput');

            // Only request what the user asked for AND what the hardware actually supports
            const audioToRequest = requestAudio && hasAudioDevice;
            const videoToRequest = requestVideo && hasVideoDevice;

            if (!audioToRequest && !videoToRequest) {
                return { success: false, hasAudio: false, hasVideo: false };
            }

            localStream = await navigator.mediaDevices.getUserMedia({ 
                audio: audioToRequest, 
                video: videoToRequest 
            });

            // Double check what we actually got
            const actualAudio = localStream.getAudioTracks().length > 0;
            const actualVideo = localStream.getVideoTracks().length > 0;

            return { 
                success: true, 
                hasAudio: actualAudio, 
                hasVideo: actualVideo 
            };
        } catch (e) {
            console.error('Error getting local stream:', e);
            // Return failure
            return { success: false, hasAudio: false, hasVideo: false };
        }
    },

    stopLocalStream: () => {
        if (localStream) {
            localStream.getTracks().forEach(track => track.stop());
            localStream = null;
        }
    },

    toggleLocalVideo: (enabled) => {
        if (localStream) {
            localStream.getVideoTracks().forEach(t => t.enabled = enabled);
        }
    },

    toggleLocalAudio: (enabled) => {
        if (localStream) {
            localStream.getAudioTracks().forEach(t => t.enabled = enabled);
        }
    },

    createPeerConnection: async (targetUserId, isInitiator) => {
        if (peerConnections[targetUserId]) return;

        const pc = new RTCPeerConnection(configuration);
        peerConnections[targetUserId] = pc;

        // Add local tracks
        localStream.getTracks().forEach(track => pc.addTrack(track, localStream));

        pc.onicecandidate = (event) => {
            if (event.candidate) {
                dotNetHelper.invokeMethodAsync('SendSignal', targetUserId, JSON.stringify({ candidate: event.candidate }));
            }
        };

        pc.ontrack = (event) => {
            dotNetHelper.invokeMethodAsync('OnRemoteTrackReceived', targetUserId, event.streams[0].id);
            // We'll attach the stream to a video element in Blazor
            const videoEl = document.getElementById(`video-${targetUserId}`);
            if (videoEl) {
                videoEl.srcObject = event.streams[0];
            }
        };

        if (isInitiator) {
            const offer = await pc.createOffer();
            await pc.setLocalDescription(offer);
            dotNetHelper.invokeMethodAsync('SendSignal', targetUserId, JSON.stringify({ sdp: pc.localDescription }));
        }
    },

    handleSignal: async (fromUserId, signalStr) => {
        const signal = JSON.parse(signalStr);
        let pc = peerConnections[fromUserId];

        if (!pc) {
            // If we receive a signal but don't have a PC, create one (not as initiator)
            await window.webrtc.createPeerConnection(fromUserId, false);
            pc = peerConnections[fromUserId];
        }

        if (signal.sdp) {
            await pc.setRemoteDescription(new RTCSessionDescription(signal.sdp));
            if (pc.remoteDescription.type === 'offer') {
                const answer = await pc.createAnswer();
                await pc.setLocalDescription(answer);
                dotNetHelper.invokeMethodAsync('SendSignal', fromUserId, JSON.stringify({ sdp: pc.localDescription }));
            }
        } else if (signal.candidate) {
            await pc.addIceCandidate(new RTCIceCandidate(signal.candidate));
        }
    },

    closePeerConnection: (userId) => {
        if (peerConnections[userId]) {
            peerConnections[userId].close();
            delete peerConnections[userId];
        }
    },

    startScreenShare: async () => {
        try {
            screenStream = await navigator.mediaDevices.getDisplayMedia({ video: true });
            const videoTrack = screenStream.getVideoTracks()[0];
            
            // Replace video tracks in all peer connections
            for (const userId in peerConnections) {
                const pc = peerConnections[userId];
                const sender = pc.getSenders().find(s => s.track.kind === 'video');
                if (sender) {
                    sender.replaceTrack(videoTrack);
                }
            }

            videoTrack.onended = () => {
                window.webrtc.stopScreenShare();
            };

            return true;
        } catch (e) {
            console.error('Error sharing screen:', e);
            return false;
        }
    },

    stopScreenShare: async () => {
        if (screenStream) {
            screenStream.getTracks().forEach(t => t.stop());
            screenStream = null;

            // Revert to local camera track if it exists
            const cameraTrack = (localStream && localStream.getVideoTracks().length > 0) 
                ? localStream.getVideoTracks()[0] 
                : null;

            for (const userId in peerConnections) {
                const pc = peerConnections[userId];
                const sender = pc.getSenders().find(s => s.track && s.track.kind === 'video');
                if (sender) {
                    // If no camera track, we send null (black screen) or could stop the sender
                    sender.replaceTrack(cameraTrack);
                }
            }
        }
    },

    attachLocalStream: (elementId) => {
        const videoEl = document.getElementById(elementId);
        if (videoEl && localStream) {
            videoEl.srcObject = localStream;
        } else if (videoEl) {
            videoEl.srcObject = null;
        }
    },

    startAudioAnalysis: () => {
        if (!localStream) return;
        audioContext = new AudioContext();
        analyser = audioContext.createAnalyser();
        microphone = audioContext.createMediaStreamSource(localStream);
        javascriptNode = audioContext.createScriptProcessor(2048, 1, 1);

        analyser.smoothingTimeConstant = 0.8;
        analyser.fftSize = 1024;

        microphone.connect(analyser);
        analyser.connect(javascriptNode);
        javascriptNode.connect(audioContext.destination);

        javascriptNode.onaudioprocess = () => {
            const array = new Uint8Array(analyser.frequencyBinCount);
            analyser.getByteFrequencyData(array);
            let values = 0;
            const length = array.length;
            for (let i = 0; i < length; i++) {
                values += (array[i]);
            }
            const average = values / length;
            // Notify Blazor only if threshold reached to save bandwidth
            if (average > 10) {
                dotNetHelper.invokeMethodAsync('OnLocalVolumeChanged', average);
            }
        };
    },

    stopAudioAnalysis: () => {
        if (audioContext) {
            audioContext.close();
            audioContext = null;
        }
    }
};