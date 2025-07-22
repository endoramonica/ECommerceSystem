// Global variables
let currentStep = 1;
const totalSteps = 4;
let userData = {
    fullName: '',
    email: '',
    phone: '',
    location: '',
    petTypes: [],
    experience: ''
};

// DOM elements
const stepContents = document.querySelectorAll('.step-content');
const progressSteps = document.querySelectorAll('.progress-step');
const prevBtn = document.getElementById('prevBtn');
const nextBtn = document.getElementById('nextBtn');
const completeBtn = document.getElementById('completeBtn');

// Initialize the onboarding
document.addEventListener('DOMContentLoaded', function () {
    initializeOnboarding();
    setupEventListeners();
});

function initializeOnboarding() {
    updateStepVisibility();
    updateProgressBar();
    updateNavigationButtons();
}

function setupEventListeners() {
    // Pet type selection
    const petTypeCards = document.querySelectorAll('.pet-type-card');
    petTypeCards.forEach(card => {
        card.addEventListener('click', function () {
            togglePetType(this);
        });
    });

    // Form validation
    const formInputs = document.querySelectorAll('input, select');
    formInputs.forEach(input => {
        input.addEventListener('change', validateCurrentStep);
        input.addEventListener('input', validateCurrentStep);
    });

    // Experience level selection
    const experienceInputs = document.querySelectorAll('input[name="experience"]');
    experienceInputs.forEach(input => {
        input.addEventListener('change', function () {
            userData.experience = this.value;
            validateCurrentStep();
        });
    });
}

function nextStep() {
    if (currentStep < totalSteps) {
        // Validate current step before proceeding
        if (!validateCurrentStep()) {
            return;
        }

        // Save current step data
        saveStepData();

        // Move to next step
        currentStep++;
        updateStepVisibility();
        updateProgressBar();
        updateNavigationButtons();

        // Update summary if on final step
        if (currentStep === totalSteps) {
            updateSummary();
        }
    }
}

function previousStep() {
    if (currentStep > 1) {
        currentStep--;
        updateStepVisibility();
        updateProgressBar();
        updateNavigationButtons();
    }
}

function updateStepVisibility() {
    stepContents.forEach((content, index) => {
        content.classList.toggle('active', index + 1 === currentStep);
    });
}

function updateProgressBar() {
    progressSteps.forEach((step, index) => {
        const stepNumber = index + 1;
        step.classList.toggle('active', stepNumber === currentStep);
        step.classList.toggle('completed', stepNumber < currentStep);
    });
}

function updateNavigationButtons() {
    // Previous button
    prevBtn.style.display = currentStep > 1 ? 'inline-flex' : 'none';

    // Next button
    nextBtn.style.display = currentStep < totalSteps ? 'inline-flex' : 'none';

    // Complete button
    completeBtn.style.display = currentStep === totalSteps ? 'inline-flex' : 'none';

    // Validate current step to enable/disable next button
    validateCurrentStep();
}

function validateCurrentStep() {
    let isValid = true;

    switch (currentStep) {
        case 1:
            // Welcome step - always valid
            isValid = true;
            break;

        case 2:
            // User info step
            const fullName = document.getElementById('fullName').value.trim();
            const email = document.getElementById('email').value.trim();
            const phone = document.getElementById('phone').value.trim();
            const location = document.getElementById('location').value;

            isValid = fullName && email && phone && location && validateEmail(email);
            break;

        case 3:
            // Preferences step
            const selectedPetTypes = document.querySelectorAll('.pet-type-card.selected');
            const selectedExperience = document.querySelector('input[name="experience"]:checked');

            isValid = selectedPetTypes.length > 0 && selectedExperience;
            break;

        case 4:
            // Complete step - always valid
            isValid = true;
            break;
    }

    // Enable/disable next button based on validation
    if (nextBtn) {
        nextBtn.disabled = !isValid;
    }

    return isValid;
}

function validateEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

function saveStepData() {
    switch (currentStep) {
        case 2:
            userData.fullName = document.getElementById('fullName').value.trim();
            userData.email = document.getElementById('email').value.trim();
            userData.phone = document.getElementById('phone').value.trim();
            userData.location = document.getElementById('location').value;
            break;

        case 3:
            // Save selected pet types
            const selectedPetTypes = document.querySelectorAll('.pet-type-card.selected');
            userData.petTypes = Array.from(selectedPetTypes).map(card => card.dataset.type);

            // Save experience level
            const selectedExperience = document.querySelector('input[name="experience"]:checked');
            userData.experience = selectedExperience ? selectedExperience.value : '';
            break;
    }
}

function togglePetType(card) {
    card.classList.toggle('selected');
    validateCurrentStep();
}

function updateSummary() {
    document.getElementById('summary-name').textContent = userData.fullName;
    document.getElementById('summary-email').textContent = userData.email;

    // Format pet types
    const petTypeNames = {
        dog: 'Chó',
        cat: 'Mèo',
        bird: 'Chim',
        rabbit: 'Thỏ',
        hamster: 'Hamster',
        fish: 'Cá'
    };

    const petTypesText = userData.petTypes.map(type => petTypeNames[type]).join(', ');
    document.getElementById('summary-pets').textContent = petTypesText;

    // Format experience
    const experienceNames = {
        beginner: 'Mới bắt đầu',
        intermediate: 'Có kinh nghiệm',
        expert: 'Rất có kinh nghiệm'
    };

    document.getElementById('summary-experience').textContent = experienceNames[userData.experience];
}

function completeOnboarding() {
    // Show loading state
    completeBtn.disabled = true;
    completeBtn.textContent = 'Đang xử lý...';

    // Simulate API call
    setTimeout(() => {
        // Here you would typically send userData to your server
        console.log('User data to be saved:', userData);

        // Show success message
        showSuccessMessage();

        // Redirect to main app after a delay
        setTimeout(() => {
            // Replace with your actual redirect URL
            window.location.href = '/Home/Index';
        }, 2000);
    }, 1500);
}

function showSuccessMessage() {
    // Create success overlay
    const overlay = document.createElement('div');
    overlay.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(0, 0, 0, 0.8);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 1000;
    `;

    const message = document.createElement('div');
    message.style.cssText = `
        background: white;
        padding: 40px;
        border-radius: 20px;
        text-align: center;
        max-width: 400px;
        animation: fadeIn 0.5s ease-in-out;
    `;

    message.innerHTML = `
        <div style="font-size: 60px; margin-bottom: 20px;">✅</div>
        <h2 style="color: #4ECDC4; margin-bottom: 15px;">Thành công!</h2>
        <p style="color: #666;">Đang chuyển hướng đến ứng dụng...</p>
    `;

    overlay.appendChild(message);
    document.body.appendChild(overlay);
}

// Utility functions for animations
function animateStepTransition() {
    const activeStep = document.querySelector('.step-content.active');
    if (activeStep) {
        activeStep.style.animation = 'none';
        setTimeout(() => {
            activeStep.style.animation = 'fadeIn 0.5s ease-in-out';
        }, 10);
    }
}

// Handle form submission with Enter key
document.addEventListener('keypress', function (e) {
    if (e.key === 'Enter') {
        e.preventDefault();
        if (currentStep < totalSteps && validateCurrentStep()) {
            nextStep();
        } else if (currentStep === totalSteps) {
            completeOnboarding();
        }
    }
});

// Handle browser back/forward buttons
window.addEventListener('popstate', function (e) {
    if (e.state && e.state.step) {
        currentStep = e.state.step;
        updateStepVisibility();
        updateProgressBar();
        updateNavigationButtons();
    }
});

// Add state to browser history for each step
function addToHistory() {
    const state = { step: currentStep };
    const url = `${window.location.pathname}?step=${currentStep}`;
    history.pushState(state, '', url);
}

// Auto-save user data to prevent data loss
function autoSaveData() {
    const tempData = {
        fullName: document.getElementById('fullName')?.value || '',
        email: document.getElementById('email')?.value || '',
        phone: document.getElementById('phone')?.value || '',
        location: document.getElementById('location')?.value || '',
        petTypes: Array.from(document.querySelectorAll('.pet-type-card.selected')).map(card => card.dataset.type),
        experience: document.querySelector('input[name="experience"]:checked')?.value || ''
    };

    // In a real application, you might want to save this to localStorage
    // or send it to a temporary storage endpoint
    console.log('Auto-saving data:', tempData);
}

// Set up auto-save interval
setInterval(autoSaveData, 30000); // Auto-save every 30 seconds

// Handle page visibility changes
document.addEventListener('visibilitychange', function () {
    if (document.hidden) {
        autoSaveData();
    }
});

// Prevent accidental page refresh
window.addEventListener('beforeunload', function (e) {
    if (currentStep > 1 && currentStep < totalSteps) {
        e.preventDefault();
        e.returnValue = '';
        return '';
    }
});

// Initialize tooltips and help text
function initializeHelpSystem() {
    const helpButtons = document.querySelectorAll('[data-help]');
    helpButtons.forEach(button => {
        button.addEventListener('click', function () {
            showHelpModal(this.dataset.help);
        });
    });
}

function showHelpModal(helpText) {
    const modal = document.createElement('div');
    modal.className = 'help-modal';
    modal.innerHTML = `
        <div class="help-modal-content">
            <div class="help-modal-header">
                <h3>Trợ giúp</h3>
                <button class="close-help" onclick="closeHelpModal()">&times;</button>
            </div>
            <div class="help-modal-body">
                <p>${helpText}</p>
            </div>
        </div>
    `;
    document.body.appendChild(modal);
}

function closeHelpModal() {
    const modal = document.querySelector('.help-modal');
    if (modal) {
        modal.remove();
    }
}

// Form validation with real-time feedback
function setupRealTimeValidation() {
    const inputs = document.querySelectorAll('input, select');
    inputs.forEach(input => {
        input.addEventListener('blur', function () {
            validateField(this);
        });

        input.addEventListener('input', function () {
            clearFieldError(this);
        });
    });
}

function validateField(field) {
    let isValid = true;
    let errorMessage = '';

    switch (field.type) {
        case 'email':
            if (!validateEmail(field.value)) {
                isValid = false;
                errorMessage = 'Email không hợp lệ';
            }
            break;

        case 'tel':
            if (!validatePhone(field.value)) {
                isValid = false;
                errorMessage = 'Số điện thoại không hợp lệ';
            }
            break;

        case 'text':
            if (field.value.trim().length < 2) {
                isValid = false;
                errorMessage = 'Vui lòng nhập ít nhất 2 ký tự';
            }
            break;
    }

    if (!isValid) {
        showFieldError(field, errorMessage);
    } else {
        clearFieldError(field);
    }

    return isValid;
}

function validatePhone(phone) {
    const phoneRegex = /^(\+84|0)[3|5|7|8|9][0-9]{8}$/;
    return phoneRegex.test(phone.replace(/\s/g, ''));
}

function showFieldError(field, message) {
    clearFieldError(field);

    const errorDiv = document.createElement('div');
    errorDiv.className = 'field-error';
    errorDiv.textContent = message;
    errorDiv.style.cssText = `
        color: #dc3545;
        font-size: 12px;
        margin-top: 5px;
        animation: fadeIn 0.3s ease-in-out;
    `;

    field.parentNode.appendChild(errorDiv);
    field.style.borderColor = '#dc3545';
}

function clearFieldError(field) {
    const errorDiv = field.parentNode.querySelector('.field-error');
    if (errorDiv) {
        errorDiv.remove();
    }
    field.style.borderColor = '#E0E0E0';
}

// Initialize all systems when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    initializeHelpSystem();
    setupRealTimeValidation();

    // Add smooth scrolling for better UX
    document.documentElement.style.scrollBehavior = 'smooth';
});

// Accessibility improvements
function setupAccessibility() {
    // Add ARIA labels
    const steps = document.querySelectorAll('.step-content');
    steps.forEach((step, index) => {
        step.setAttribute('aria-label', `Bước ${index + 1} của ${totalSteps}`);
    });

    // Add keyboard navigation
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Tab') {
            // Custom tab navigation if needed
        }
    });
}

// Performance optimization
function optimizePerformance() {
    // Lazy load images if any
    const images = document.querySelectorAll('img[data-src]');
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src;
                    img.removeAttribute('data-src');
                    imageObserver.unobserve(img);
                }
            });
        });

        images.forEach(img => imageObserver.observe(img));
    }
}

// Initialize all features
document.addEventListener('DOMContentLoaded', function () {
    setupAccessibility();
    optimizePerformance();
});