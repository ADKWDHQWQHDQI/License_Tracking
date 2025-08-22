// CBMS Authentication JavaScript
// Enhanced security and user experience for login/register pages

document.addEventListener('DOMContentLoaded', function() {
    initAuthPage();
    initFormValidation();
    initSecurityFeatures();
    initAccessibility();
});

// Initialize authentication page
function initAuthPage() {
    // Add loading states to buttons
    document.querySelectorAll('.auth-btn').forEach(button => {
        button.addEventListener('click', function(e) {
            if (this.type === 'submit') {
                const form = this.closest('form');
                if (form && form.checkValidity()) {
                    showLoadingState(this);
                }
            }
        });
    });

    // Brand icon animation
    const brandIcon = document.querySelector('.brand-icon');
    if (brandIcon) {
        setInterval(() => {
            brandIcon.style.transform = 'scale(1.05)';
            setTimeout(() => {
                brandIcon.style.transform = 'scale(1)';
            }, 200);
        }, 3000);
    }

    // Background animation
    initBackgroundAnimation();
}

// Enhanced form validation
function initFormValidation() {
    const forms = document.querySelectorAll('.auth-form form');
    
    forms.forEach(form => {
        form.addEventListener('submit', function(e) {
            e.preventDefault();
            
            if (validateForm(this)) {
                // Form is valid, submit it
                this.submit();
            }
        });

        // Real-time validation
        const inputs = form.querySelectorAll('.auth-form-control');
        inputs.forEach(input => {
            input.addEventListener('blur', () => validateField(input));
            input.addEventListener('input', () => clearFieldError(input));
        });
    });
}

// Form validation logic
function validateForm(form) {
    let isValid = true;
    const inputs = form.querySelectorAll('.auth-form-control[required]');
    
    inputs.forEach(input => {
        if (!validateField(input)) {
            isValid = false;
        }
    });

    // Additional validation for registration form
    if (form.querySelector('#Role')) {
        const roleSelect = form.querySelector('#Role');
        if (!roleSelect.value) {
            showFieldError(roleSelect, 'Please select your role');
            isValid = false;
        }
    }

    // Terms checkbox validation
    const termsCheckbox = form.querySelector('input[type="checkbox"][required]');
    if (termsCheckbox && !termsCheckbox.checked) {
        showFieldError(termsCheckbox, 'You must agree to the terms and conditions');
        isValid = false;
    }

    return isValid;
}

// Field validation
function validateField(input) {
    const value = input.value.trim();
    const type = input.type;
    let isValid = true;
    let errorMessage = '';

    // Required field validation
    if (input.hasAttribute('required') && !value) {
        errorMessage = `${getFieldName(input)} is required`;
        isValid = false;
    }
    // Email validation
    else if (type === 'email' && value) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(value)) {
            errorMessage = 'Please enter a valid email address';
            isValid = false;
        }
    }
    // Password validation
    else if (type === 'password' && value && input.id === 'passwordInput') {
        const passwordValidation = validatePassword(value);
        if (!passwordValidation.isValid) {
            errorMessage = passwordValidation.message;
            isValid = false;
        }
    }

    if (!isValid) {
        showFieldError(input, errorMessage);
    } else {
        clearFieldError(input);
    }

    return isValid;
}

// Password validation
function validatePassword(password) {
    const minLength = 8;
    const hasUppercase = /[A-Z]/.test(password);
    const hasLowercase = /[a-z]/.test(password);
    const hasNumber = /\d/.test(password);
    const hasSpecial = /[!@#$%^&*(),.?":{}|<>]/.test(password);

    if (password.length < minLength) {
        return { isValid: false, message: `Password must be at least ${minLength} characters long` };
    }
    if (!hasUppercase) {
        return { isValid: false, message: 'Password must contain at least one uppercase letter' };
    }
    if (!hasLowercase) {
        return { isValid: false, message: 'Password must contain at least one lowercase letter' };
    }
    if (!hasNumber) {
        return { isValid: false, message: 'Password must contain at least one number' };
    }
    if (!hasSpecial) {
        return { isValid: false, message: 'Password must contain at least one special character' };
    }

    return { isValid: true, message: '' };
}

// Utility functions
function getFieldName(input) {
    const label = document.querySelector(`label[for="${input.id}"]`);
    if (label) {
        return label.textContent.replace(/[^\w\s]/gi, '').trim();
    }
    return input.name || input.id || 'Field';
}

function showFieldError(input, message) {
    input.classList.add('input-validation-error');
    
    let errorElement = input.parentNode.querySelector('.field-validation-error');
    if (!errorElement) {
        errorElement = document.createElement('span');
        errorElement.className = 'field-validation-error';
        input.parentNode.appendChild(errorElement);
    }
    
    errorElement.textContent = message;
}

function clearFieldError(input) {
    input.classList.remove('input-validation-error');
    
    const errorElement = input.parentNode.querySelector('.field-validation-error');
    if (errorElement) {
        errorElement.textContent = '';
    }
}

function showLoadingState(button) {
    const originalText = button.innerHTML;
    button.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Please wait...';
    button.disabled = true;
    
    // Restore button after 30 seconds (timeout)
    setTimeout(() => {
        button.innerHTML = originalText;
        button.disabled = false;
    }, 30000);
}

// Security features
function initSecurityFeatures() {
    // Disable right-click context menu on sensitive areas
    document.querySelectorAll('.auth-form').forEach(form => {
        form.addEventListener('contextmenu', function(e) {
            e.preventDefault();
        });
    });

    // Disable developer tools shortcuts
    document.addEventListener('keydown', function(e) {
        // Disable F12, Ctrl+Shift+I, Ctrl+Shift+J, Ctrl+U
        if (e.key === 'F12' || 
            (e.ctrlKey && e.shiftKey && (e.key === 'I' || e.key === 'J')) ||
            (e.ctrlKey && e.key === 'U')) {
            e.preventDefault();
            return false;
        }
    });

    // Clear form data on page unload for security
    window.addEventListener('beforeunload', function() {
        document.querySelectorAll('.auth-form-control').forEach(input => {
            if (input.type === 'password') {
                input.value = '';
            }
        });
    });

    // Session timeout warning
    let sessionTimeout = 30 * 60 * 1000; // 30 minutes
    let warningShown = false;
    
    setTimeout(() => {
        if (!warningShown) {
            warningShown = true;
            if (confirm('Your session will expire soon. Would you like to continue?')) {
                // Reset timeout
                warningShown = false;
                setTimeout(arguments.callee, sessionTimeout);
            } else {
                window.location.href = '/Auth/Login';
            }
        }
    }, sessionTimeout - 5 * 60 * 1000); // Show warning 5 minutes before timeout
}

// Accessibility features
function initAccessibility() {
    // Add ARIA labels to form elements
    document.querySelectorAll('.auth-form-control').forEach(input => {
        if (!input.getAttribute('aria-label')) {
            const label = document.querySelector(`label[for="${input.id}"]`);
            if (label) {
                input.setAttribute('aria-label', label.textContent.trim());
            }
        }
    });

    // Keyboard navigation improvements
    document.addEventListener('keydown', function(e) {
        // Enter key to submit form
        if (e.key === 'Enter' && e.target.classList.contains('auth-form-control')) {
            const form = e.target.closest('form');
            if (form) {
                const submitButton = form.querySelector('button[type="submit"]');
                if (submitButton) {
                    submitButton.click();
                }
            }
        }
    });

    // Focus management
    const firstInput = document.querySelector('.auth-form-control');
    if (firstInput) {
        setTimeout(() => firstInput.focus(), 100);
    }
}

// Background animation
function initBackgroundAnimation() {
    const background = document.querySelector('.auth-background::before');
    if (background) {
        // Add subtle movement to background gradients
        setInterval(() => {
            const randomTransform = `translate(${Math.random() * 20 - 10}px, ${Math.random() * 20 - 10}px)`;
            background.style.transform = randomTransform;
        }, 5000);
    }
}

// Branding interactions
document.querySelectorAll('.brand-link').forEach(link => {
    link.addEventListener('mouseenter', function() {
        const icon = this.querySelector('.brand-icon');
        if (icon) {
            icon.style.transform = 'scale(1.1) rotate(5deg)';
        }
    });
    
    link.addEventListener('mouseleave', function() {
        const icon = this.querySelector('.brand-icon');
        if (icon) {
            icon.style.transform = 'scale(1) rotate(0deg)';
        }
    });
});

// Form animations
document.querySelectorAll('.auth-form-control').forEach(input => {
    input.addEventListener('focus', function() {
        this.parentNode.classList.add('focused');
    });
    
    input.addEventListener('blur', function() {
        this.parentNode.classList.remove('focused');
    });
});

// Error handling
window.addEventListener('error', function(e) {
    console.warn('Auth page error:', e.error);
});

// Export functions for external use
window.AuthPage = {
    validateForm,
    validateField,
    validatePassword,
    showLoadingState,
    initSecurityFeatures
};
