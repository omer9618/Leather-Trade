// Function to show payment status
function showPaymentStatus(message, type = 'info') {
    const statusDiv = document.getElementById('payment-status');
    const statusMessage = document.getElementById('status-message');
    statusDiv.className = `alert alert-${type} mb-4`;
    statusMessage.textContent = message;
    statusDiv.style.display = 'block';
}

// Function to show success state
function showSuccessState() {
    const form = document.getElementById('payment-form');
    const successDiv = document.getElementById('payment-success');
    const statusDiv = document.getElementById('payment-status');
    
    form.style.display = 'none';
    successDiv.style.display = 'block';
    statusDiv.style.display = 'none';
}

// Function to handle API response
async function handleApiResponse(response) {
    const data = await response.json();
    if (!response.ok) {
        throw new Error(data.message || `Server error: ${response.status}`);
    }
    return data;
}

document.addEventListener('DOMContentLoaded', function() {
    var form = document.getElementById('payment-form');
    if (!form) return;

    form.addEventListener('submit', function(event) {
        event.preventDefault();

        var submitButton = document.getElementById('submit-payment');
        var spinner = document.getElementById('spinner');
        var buttonText = document.getElementById('button-text');
        var bidId = document.getElementById('bid-id').value;

        if (!bidId) {
            alert('Bid ID is missing.');
            return;
        }

        submitButton.disabled = true;
        spinner.classList.remove('hidden');
        buttonText.textContent = 'Processing...';
        showPaymentStatus('Processing your payment...', 'info');

        // Since this is purely a test-driven mock form, we just hit ConfirmPayment 
        // with the bypass ID right away.
        fetch('/Payment/ConfirmPayment', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({
                PaymentIntentId: 'pi_test_bypass_' + Date.now(),
                BidId: parseInt(bidId)
            })
        })
        .then(handleApiResponse)
        .then(data => {
            if (data.success) {
                showSuccessState();
            } else {
                throw new Error(data.message);
            }
        })
        .catch(error => {
            console.error('Payment error:', error);
            submitButton.disabled = false;
            spinner.classList.add('hidden');
            buttonText.textContent = 'Pay';
            showPaymentStatus('Payment failed: ' + error.message, 'danger');
        });
    });
}); 