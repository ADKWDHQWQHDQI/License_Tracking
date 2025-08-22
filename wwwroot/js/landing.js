// CBMS Landing Page JavaScript
// Enhanced user experience and interactions

document.addEventListener('DOMContentLoaded', function() {
    // Initialize all landing page functionality
    initNavigation();
    initAnimations();
    initSmoothScrolling();
    initFormInteractions();
});

// Navigation functionality
function initNavigation() {
    const navbar = document.querySelector('.navbar-landing');
    
    // Handle navbar scroll effect
    window.addEventListener('scroll', function() {
        if (window.scrollY > 50) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
    });

    // Mobile menu toggle
    const navbarToggler = document.querySelector('.navbar-toggler');
    const navbarCollapse = document.querySelector('.navbar-collapse');
    
    if (navbarToggler && navbarCollapse) {
        navbarToggler.addEventListener('click', function() {
            navbarCollapse.classList.toggle('show');
        });
    }

    // Close mobile menu when clicking on nav links
    document.querySelectorAll('.navbar-nav .nav-link').forEach(link => {
        link.addEventListener('click', function() {
            navbarCollapse.classList.remove('show');
        });
    });
}

// Animation functionality
function initAnimations() {
    // Intersection Observer for scroll animations
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver(function(entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate__animated', 'animate__fadeInUp');
            }
        });
    }, observerOptions);

    // Observe all feature cards, benefit items, and workflow steps
    document.querySelectorAll('.feature-card, .benefit-item, .workflow-step').forEach(el => {
        observer.observe(el);
    });

    // Counter animation for hero stats
    animateCounters();
}

// Smooth scrolling for anchor links
function initSmoothScrolling() {
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            
            if (target) {
                const headerOffset = 80;
                const elementPosition = target.getBoundingClientRect().top;
                const offsetPosition = elementPosition + window.pageYOffset - headerOffset;

                window.scrollTo({
                    top: offsetPosition,
                    behavior: 'smooth'
                });
            }
        });
    });
}

// Form interactions
function initFormInteractions() {
    // Add hover effects to CTA buttons
    document.querySelectorAll('.btn').forEach(button => {
        button.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-2px)';
        });
        
        button.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
        });
    });

    // Track CTA button clicks
    document.querySelectorAll('[href*="Register"], [href*="Login"]').forEach(link => {
        link.addEventListener('click', function() {
            // Analytics tracking would go here
            console.log('CTA clicked:', this.textContent.trim());
        });
    });
}

// Counter animation
function animateCounters() {
    const counters = document.querySelectorAll('.hero-stat-number');
    
    const animateCounter = (counter) => {
        const target = counter.textContent.trim();
        const isNumber = /^\d+/.test(target);
        
        if (isNumber) {
            const finalValue = parseInt(target.replace(/\D/g, ''));
            const duration = 2000; // 2 seconds
            const increment = finalValue / (duration / 16); // 60fps
            let current = 0;
            
            const timer = setInterval(() => {
                current += increment;
                if (current >= finalValue) {
                    current = finalValue;
                    clearInterval(timer);
                }
                
                const suffix = target.includes('+') ? '+' : 
                              target.includes('M') ? 'M+' : 
                              target.includes('%') ? '%' : '';
                              
                counter.textContent = Math.floor(current) + suffix;
            }, 16);
        }
    };

    // Trigger counter animation when hero section is visible
    const heroObserver = new IntersectionObserver(function(entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                counters.forEach(counter => animateCounter(counter));
                heroObserver.unobserve(entry.target);
            }
        });
    }, { threshold: 0.5 });

    const heroSection = document.querySelector('.hero-section');
    if (heroSection) {
        heroObserver.observe(heroSection);
    }
}

// Feature card interactions
document.querySelectorAll('.feature-card').forEach(card => {
    card.addEventListener('mouseenter', function() {
        const icon = this.querySelector('.feature-icon');
        if (icon) {
            icon.style.transform = 'scale(1.1) rotate(5deg)';
        }
    });
    
    card.addEventListener('mouseleave', function() {
        const icon = this.querySelector('.feature-icon');
        if (icon) {
            icon.style.transform = 'scale(1) rotate(0deg)';
        }
    });
});

// Workflow step interactions
document.querySelectorAll('.workflow-step').forEach((step, index) => {
    step.addEventListener('mouseenter', function() {
        const number = this.querySelector('.workflow-number');
        if (number) {
            number.style.transform = 'scale(1.1)';
            number.style.boxShadow = '0 8px 25px rgba(59, 130, 246, 0.4)';
        }
    });
    
    step.addEventListener('mouseleave', function() {
        const number = this.querySelector('.workflow-number');
        if (number) {
            number.style.transform = 'scale(1)';
            number.style.boxShadow = '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)';
        }
    });
});

// Parallax effect for hero section
window.addEventListener('scroll', function() {
    const scrolled = window.pageYOffset;
    const parallax = document.querySelector('.hero-section');
    
    if (parallax) {
        const speed = 0.5;
        parallax.style.transform = `translateY(${scrolled * speed}px)`;
    }
});

// Page load performance optimization
window.addEventListener('load', function() {
    // Preload critical images
    const criticalImages = [
        '/images/hero-bg.jpg',
        '/images/feature-icons.svg'
    ];
    
    criticalImages.forEach(src => {
        const img = new Image();
        img.src = src;
    });
});

// Error handling
window.addEventListener('error', function(e) {
    console.warn('Landing page error:', e.error);
});

// Accessibility improvements
document.addEventListener('keydown', function(e) {
    // Skip to main content with Tab key
    if (e.key === 'Tab' && !e.shiftKey && document.activeElement.tagName === 'BODY') {
        const mainContent = document.querySelector('main');
        if (mainContent) {
            mainContent.focus();
            e.preventDefault();
        }
    }
});

// Export functions for external use
window.LandingPage = {
    initNavigation,
    initAnimations,
    initSmoothScrolling,
    initFormInteractions
};
