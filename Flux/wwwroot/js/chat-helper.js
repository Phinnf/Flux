window.chatHelper = {
    autoResizeTextArea: function (element) {
        if (!element) return;
        element.style.height = 'auto'; // Reset to calculate true scrollHeight
        element.style.height = element.scrollHeight + 'px';
    },
    insertTextAtCursor: function (element, prefix, suffix) {
        if (!element) return null;
        
        let startPos = element.selectionStart;
        let endPos = element.selectionEnd;
        let value = element.value;
        let selectedText = value.substring(startPos, endPos);
        
        let newText = value.substring(0, startPos) + prefix + selectedText + suffix + value.substring(endPos, value.length);
        
        // Update the textarea value
        element.value = newText;
        
        // Dispatch input event to notify Blazor of the value change
        element.dispatchEvent(new Event('input', { bubbles: true }));
        
        // Move cursor to between prefix and suffix if nothing was selected,
        // or to the end of the newly inserted text if something was selected.
        let newCursorPos = startPos + prefix.length;
        if (selectedText.length > 0) {
            newCursorPos += selectedText.length + suffix.length;
        }
        
        element.setSelectionRange(newCursorPos, newCursorPos);
        element.focus();
        
        return newText;
    },
    resetTextAreaHeight: function(element) {
        if (!element) return;
        element.value = ''; // Force clear the value in the DOM
        element.style.height = 'auto';
    },
    copyToClipboard: async function(text) {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch (err) {
            console.error('Failed to copy: ', err);
            return false;
        }
    }
};