// Create and append the chat button
function createChatButton() {
    const button = document.createElement('button');
    button.id = 'aiChatButton';
    button.innerHTML = '<i class="fas fa-robot"></i><span>AI Chat</span>';
    button.onclick = function() {
        alert('AI Chat clicked!');
    };
    document.body.appendChild(button);
}

// Add styles for the chat button
function addChatButtonStyles() {
    const style = document.createElement('style');
    style.textContent = `
        #aiChatButton {
            position: fixed !important;
            bottom: 20px !important;
            right: 20px !important;
            background: #007bff !important;
            color: white !important;
            border: none !important;
            padding: 12px 24px !important;
            border-radius: 25px !important;
            cursor: pointer !important;
            z-index: 9999 !important;
            display: flex !important;
            align-items: center !important;
            gap: 10px !important;
            font-size: 16px !important;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1) !important;
            transition: all 0.3s ease !important;
        }

        #aiChatButton:hover {
            background: #0056b3 !important;
            transform: scale(1.05) !important;
        }
    `;
    document.head.appendChild(style);
}

// Initialize when the DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    console.log('Initializing chat button...');
    addChatButtonStyles();
    createChatButton();
    console.log('Chat button should be visible now');
}); 