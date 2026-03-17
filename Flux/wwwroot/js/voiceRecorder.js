
window.voiceRecorder = {
    mediaRecorder: null,
    audioChunks: [],
    dotNetReference: null,

    startRecording: async function (dotNetRef) {
        try {
            this.dotNetReference = dotNetRef;
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            this.mediaRecorder = new MediaRecorder(stream);
            this.audioChunks = [];

            this.mediaRecorder.ondataavailable = (event) => {
                this.audioChunks.push(event.data);
            };

            this.mediaRecorder.onstop = async () => {
                const audioBlob = new Blob(this.audioChunks, { type: 'audio/webm' });
                const reader = new FileReader();
                reader.readAsDataURL(audioBlob);
                reader.onloadend = () => {
                    const base64String = reader.result.split(',')[1];
                    this.dotNetReference.invokeMethodAsync('OnAudioDataReceived', base64String);
                };

                // Stop all tracks to release microphone
                stream.getTracks().forEach(track => track.stop());
            };

            this.mediaRecorder.start();
            return true;
        } catch (err) {
            console.error("Error accessing microphone:", err);
            return false;
        }
    },

    stopRecording: function () {
        if (this.mediaRecorder && this.mediaRecorder.state !== 'inactive') {
            this.mediaRecorder.stop();
        }
    },

    cancelRecording: function () {
        if (this.mediaRecorder && this.mediaRecorder.state !== 'inactive') {
            this.mediaRecorder.onstop = null; // Prevent callback
            this.mediaRecorder.stop();
            const tracks = this.mediaRecorder.stream.getTracks();
            tracks.forEach(track => track.stop());
        }
    }
};
