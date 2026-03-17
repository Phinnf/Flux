window.emojiPicker = {
    init: function (dotNetHelper, pickerId) {
        const picker = document.querySelector(`#${pickerId}`);
        if (!picker) return;

        // Xóa listener cũ nếu có để tránh memory leak
        if (picker._handler) {
            picker.removeEventListener('emoji-click', picker._handler);
        }

        picker._handler = (event) => {
            const emoji = event.detail.unicode; // Lấy ký tự Unicode của emoji
            // Gọi hàm C# trong Blazor
            dotNetHelper.invokeMethodAsync('OnEmojiSelectedJS', emoji);
        };

        picker.addEventListener('emoji-click', picker._handler);
    }
};