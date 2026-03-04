window.videoRecorder = {
    stream: null,
    mediaRecorder: null,
    recordedChunks: [],

    setStream: (newStream, videoElementId) => {
        // Stop existing stream if any
        if (window.videoRecorder.stream) {
            window.videoRecorder.stream.getTracks().forEach(track => track.stop());
        }
        window.videoRecorder.stream = newStream;
        const videoElement = document.getElementById(videoElementId);
        if (videoElement) {
            videoElement.srcObject = newStream;
            videoElement.muted = true; // mute local preview
            videoElement.play();
        }
    },

    toggleVideo: (enabled) => {
        if (window.videoRecorder.stream) {
            window.videoRecorder.stream.getVideoTracks().forEach(track => {
                track.enabled = enabled;
            });
        }
    },

    toggleAudio: (enabled) => {
        if (window.videoRecorder.stream) {
            window.videoRecorder.stream.getAudioTracks().forEach(track => {
                track.enabled = enabled;
            });
        }
    },

    startCamera: async (videoElementId) => {
        try {
            const stream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
            window.videoRecorder.setStream(stream, videoElementId);
            return true;
        } catch (err) {
            console.error("Error accessing camera/mic: ", err);
            return false;
        }
    },

    startScreenShare: async (videoElementId) => {
        try {
            const displayStream = await navigator.mediaDevices.getDisplayMedia({ 
                video: true, 
                audio: true 
            });
            
            try {
                const micStream = await navigator.mediaDevices.getUserMedia({ audio: true });
                if (micStream && micStream.getAudioTracks().length > 0) {
                    displayStream.addTrack(micStream.getAudioTracks()[0]);
                }
            } catch (micErr) {
                console.warn("Could not get mic to add to screen share: ", micErr);
            }

            window.videoRecorder.setStream(displayStream, videoElementId);
            return true;
        } catch (err) {
            console.error("Error accessing screen share: ", err);
            return false;
        }
    },

    stopCamera: () => {
        if (window.videoRecorder.stream) {
            window.videoRecorder.stream.getTracks().forEach(track => track.stop());
            window.videoRecorder.stream = null;
        }
    },

    startRecording: () => {
        if (!window.videoRecorder.stream) return false;
        window.videoRecorder.recordedChunks = [];
        window.videoRecorder.mediaRecorder = new MediaRecorder(window.videoRecorder.stream);
        
        window.videoRecorder.mediaRecorder.ondataavailable = (event) => {
            if (event.data.size > 0) {
                window.videoRecorder.recordedChunks.push(event.data);
            }
        };

        window.videoRecorder.mediaRecorder.start();
        return true;
    },

    stopRecording: async (videoElementId) => {
        return new Promise((resolve) => {
            if (!window.videoRecorder.mediaRecorder) {
                resolve(null);
                return;
            }

            window.videoRecorder.mediaRecorder.onstop = () => {
                const blob = new Blob(window.videoRecorder.recordedChunks, { type: 'video/webm' });
                const url = URL.createObjectURL(blob);
                
                // Play recorded video
                const videoElement = document.getElementById(videoElementId);
                if (videoElement) {
                    videoElement.srcObject = null;
                    videoElement.src = url;
                    videoElement.muted = false; // unmute so user can hear their recording
                    videoElement.controls = true;
                }
                resolve(url);
            };

            window.videoRecorder.mediaRecorder.stop();
        });
    },

    downloadVideo: (url) => {
        const a = document.createElement('a');
        a.style.display = 'none';
        a.href = url;
        a.download = `recording_${new Date().getTime()}.webm`;
        document.body.appendChild(a);
        a.click();
        setTimeout(() => {
            document.body.removeChild(a);
        }, 100);
    },

    uploadVideo: async (url, uploadEndpoint) => {
        try {
            const response = await fetch(url);
            const blob = await response.blob();
            
            const formData = new FormData();
            formData.append('file', blob, `video_${new Date().getTime()}.webm`);

            const uploadRes = await fetch(uploadEndpoint, {
                method: 'POST',
                body: formData
            });

            if (uploadRes.ok) {
                const data = await uploadRes.json();
                return data.url;
            } else {
                throw new Error('Upload failed with status ' + uploadRes.status);
            }
        } catch (error) {
            console.error('Error uploading video:', error);
            return null;
        }
    }
};