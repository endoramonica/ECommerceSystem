/* Smooth scroll for anchor links */
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
  anchor.addEventListener('click', function(e) {
    e.preventDefault();
    const target = document.querySelector(this.getAttribute('href'));
    if (target) {
      target.scrollIntoView({
        behavior: 'smooth',
        block: 'start'
      });
    }
  });
});

/* Form validation for better UX */
const forms = document.querySelectorAll('.form-control');
forms.forEach(input => {
  input.addEventListener('input', function() {
    if (this.value.trim() !== '') {
      this.classList.add('has-content');
    } else {
      this.classList.remove('has-content');
    }
  });

  input.addEventListener('blur', function() {
    if (this.type === 'email' && this.value) {
      const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailPattern.test(this.value)) {
        this.classList.add('is-invalid');
        this.setAttribute('aria-invalid', 'true');
      } else {
        this.classList.remove('is-invalid');
        this.setAttribute('aria-invalid', 'false');
      }
    }
  });
});

/* Button click animation */
const buttons = document.querySelectorAll('.btn');
buttons.forEach(btn => {
  btn.addEventListener('click', function(e) {
    if (!this.hasAttribute('disabled')) {
      this.style.transform = 'translateY(0) scale(0.98)';
      setTimeout(() => {
        this.style.transform = 'translateY(-2px) scale(1.02)';
      }, 100);
    }
  });
});

/* Card hover animation */
const cards = document.querySelectorAll('.card');
cards.forEach(card => {
  card.addEventListener('mouseenter', function() {
    this.style.transition = 'transform 0.3s ease, box-shadow 0.3s ease';
    this.style.transform = 'translateY(-5px)';
  });
  card.addEventListener('mouseleave', function() {
    this.style.transform = 'translateY(0)';
  });
});

/* Debounce function for performance */
function debounce(func, wait) {
  let timeout;
  return function executedFunction(...args) {
    const later = () => {
      clearTimeout(timeout);
      func(...args);
    };
    clearTimeout(timeout);
    timeout = setTimeout(later, wait);
  };
}

/* Handle window resize for responsive adjustments */
const handleResize = debounce(() => {
  const containers = document.querySelectorAll('.container');
  containers.forEach(container => {
    if (window.innerWidth <= 768) {
      container.style.padding = '0.5rem';
    } else {
      container.style.padding = '1rem';
    }
  });
}, 100);

window.addEventListener('resize', handleResize);
window.addEventListener('load', handleResize);

/* Accessibility: Focus management for forms */
document.addEventListener('keydown', function(e) {
  if (e.key === 'Tab') {
    const focusableElements = document.querySelectorAll(
      'a[href], button, input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );
    const firstElement = focusableElements[0];
    const lastElement = focusableElements[focusableElements.length - 1];

    if (e.shiftKey && document.activeElement === firstElement) {
      e.preventDefault();
      lastElement.focus();
    } else if (!e.shiftKey && document.activeElement === lastElement) {
      e.preventDefault();
      firstElement.focus();
    }
  }
});

/* Dark mode toggle support */
function toggleDarkMode() {
  document.documentElement.classList.toggle('dark');
  localStorage.setItem('theme', document.documentElement.classList.contains('dark') ? 'dark' : 'light');
}

// Check for saved theme preference or system preference
if (localStorage.getItem('theme') === 'dark' || 
    (!localStorage.getItem('theme') && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
  document.documentElement.classList.add('dark');
}

// Optional: Add dark mode toggle button listener
const darkModeToggle = document.querySelector('#dark-mode-toggle');
if (darkModeToggle) {
  darkModeToggle.addEventListener('click', toggleDarkMode);
}