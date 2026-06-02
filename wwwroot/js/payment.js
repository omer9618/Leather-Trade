// Function to open payment modal
function openPaymentModal(amount, bidId) {
    // Set the amount in the modal
    document.getElementById('amount').value = amount.toFixed(2);
    
    // Store the bid ID in a data attribute
    document.getElementById('payment-form').setAttribute('data-bid-id', bidId);
    
    // Show the modal
    var paymentModal = new bootstrap.Modal(document.getElementById('paymentModal'));
    paymentModal.show();
}

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

// Function to handle payment success
function handlePaymentSuccess(response) {
    if (response.success) {
        showSuccessState();
        
        // Show success message
        Swal.fire({
            icon: 'success',
            title: 'Payment Successful!',
            text: response.message || 'Your payment has been processed successfully.',
            confirmButtonText: 'OK'
        });
    } else {
        // Show error message
        Swal.fire({
            icon: 'error',
            title: 'Payment Failed',
            text: response.message || 'There was an error processing your payment.',
            confirmButtonText: 'OK'
        });
    }
}

// Function to handle payment error
function handlePaymentError(error) {
    console.error('Payment error:', error);
    let errorMessage = 'An unexpected error occurred.';
    
    if (error.message) {
        errorMessage = error.message;
    } else if (error.response) {
        errorMessage = error.response.message || 'Server error occurred';
    }
    
    Swal.fire({
        icon: 'error',
        title: 'Payment Error',
        text: errorMessage,
        confirmButtonText: 'OK'
    });
}

// Function to handle API response
async function handleApiResponse(response) {
    const data = await response.json();
    if (!response.ok) {
        throw new Error(data.message || `Server error: ${response.status}`);
    }
    return data;
}

// Initialize Stripe and form handling when the document is ready
document.addEventListener('DOMContentLoaded', function() {
    // Initialize Stripe
    var stripe = Stripe(document.getElementById('stripe-publishable-key').value);
    var elements = stripe.elements();

    // Create card Element
    var card = elements.create('card', {
        style: {
            base: {
                fontSize: '16px',
                color: '#32325d',
                fontFamily: '"Helvetica Neue", Helvetica, sans-serif',
                fontSmoothing: 'antialiased',
                '::placeholder': {
                    color: '#aab7c4'
                }
            },
            invalid: {
                color: '#fa755a',
                iconColor: '#fa755a'
            }
        }
    });

    // Mount the card Element
    card.mount('#card-element');

    // Handle real-time validation errors
    var displayError = document.getElementById('card-errors');
    card.addEventListener('change', function(event) {
        if (event.error) {
            displayError.textContent = event.error.message;
        } else {
            displayError.textContent = '';
        }
    });

    // Handle form submission
    var form = document.getElementById('payment-form');
    form.addEventListener('submit', function(event) {
        event.preventDefault();

        var submitButton = document.getElementById('submit-payment');
        var spinner = document.getElementById('spinner');
        var buttonText = document.getElementById('button-text');
        var bidId = document.getElementById('bid-id').value;

        if (!bidId) {
            handlePaymentError({ message: 'Bid ID is missing. Please try again.' });
            return;
        }

        // Disable the submit button to prevent repeated clicks
        submitButton.disabled = true;
        spinner.classList.remove('hidden');
        buttonText.textContent = 'Processing...';
        showPaymentStatus('Processing your payment...', 'info');

        stripe.createToken(card).then(function(result) {
            if (result.error) {
                // Inform the user if there was an error
                displayError.textContent = result.error.message;
                submitButton.disabled = false;
                spinner.classList.add('hidden');
                buttonText.textContent = 'Complete Payment';
                showPaymentStatus('Payment failed. Please try again.', 'danger');
            } else {
                // Send the token to your server
                var amount = document.getElementById('amount').value;

                // Create payment intent first
                fetch('/Payment/CreatePaymentIntent', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify({
                        BidId: parseInt(bidId)
                    })
                })
                .then(handleApiResponse)
                .then(data => {
                    showPaymentStatus('Confirming payment...', 'info');
                    // Directly call ConfirmPayment for demo purposes
                    return fetch('/Payment/ConfirmPayment', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                        },
                        body: JSON.stringify({
                            paymentIntentId: data.clientSecret,
                            bidId: parseInt(bidId)
                        })
                    });
                })
                .then(handleApiResponse)
                .then(data => {
                    handlePaymentSuccess(data);
                })
                .catch(error => {
                    console.error('Payment error:', error);
                    handlePaymentError(error);
                })
                .finally(() => {
                    submitButton.disabled = false;
                    spinner.classList.add('hidden');
                    buttonText.textContent = 'Complete Payment';
                });
            }
        });
    });
}); 