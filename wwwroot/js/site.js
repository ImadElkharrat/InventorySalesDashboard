// Modern UI Interactions
document.addEventListener('DOMContentLoaded', function () {
    initializeTheme();
    initializeSidebar();
    initializeAnimations();
    initializeTooltips();
});

// Theme Management
function initializeTheme() {
    const themeToggle = document.getElementById('themeToggle');
    const currentTheme = localStorage.getItem('theme') || 'dark';

    // Update toggle button icon
    updateThemeToggleIcon(currentTheme);

    if (themeToggle) {
        themeToggle.addEventListener('click', function () {
            const currentTheme = document.documentElement.getAttribute('data-bs-theme');
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';

            document.documentElement.setAttribute('data-bs-theme', newTheme);
            localStorage.setItem('theme', newTheme);
            updateThemeToggleIcon(newTheme);

            // Dispatch event for other components
            window.dispatchEvent(new CustomEvent('themeChanged', { detail: newTheme }));
        });
    }
}

function updateThemeToggleIcon(theme) {
    const themeToggle = document.getElementById('themeToggle');
    if (themeToggle) {
        const icon = themeToggle.querySelector('i');
        if (icon) {
            icon.className = theme === 'dark' ? 'fas fa-sun' : 'fas fa-moon';
        }
        themeToggle.title = `Switch to ${theme === 'dark' ? 'light' : 'dark'} theme`;
    }
}

// Sidebar Management
function initializeSidebar() {
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.querySelector('.sidebar');

    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', function () {
            sidebar.classList.toggle('collapsed');

            // Update toggle button icon
            const icon = sidebarToggle.querySelector('i');
            if (sidebar.classList.contains('collapsed')) {
                icon.className = 'fas fa-bars';
            } else {
                icon.className = 'fas fa-times';
            }
        });
    }

    // Mobile sidebar handling
    if (window.innerWidth <= 768) {
        sidebar?.classList.add('collapsed');
    }
}

// Animations
function initializeAnimations() {
    // Add entrance animations to cards
    const cards = document.querySelectorAll('.modern-card, .metric-card');
    cards.forEach((card, index) => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';

        setTimeout(() => {
            card.style.transition = 'all 0.6s cubic-bezier(0.4, 0, 0.2, 1)';
            card.style.opacity = '1';
            card.style.transform = 'translateY(0)';
        }, index * 100);
    });

    // Add hover effects to interactive elements
    const interactiveElements = document.querySelectorAll('.btn-modern, .nav-link, .metric-card');
    interactiveElements.forEach(element => {
        element.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-2px)';
        });

        element.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0)';
        });
    });
}

// Tooltips
function initializeTooltips() {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// Real-time Notification System
function showNotification(message, type = 'info', duration = 5000) {
    const notification = document.createElement('div');
    notification.className = `notification-toast alert alert-${type} alert-dismissible fade show`;
    notification.innerHTML = `
        <div class="d-flex align-items-center">
            <i class="fas fa-${getNotificationIcon(type)} me-2"></i>
            <div class="flex-grow-1">${message}</div>
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;

    document.body.appendChild(notification);

    // Auto remove
    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, duration);
}

function getNotificationIcon(type) {
    const icons = {
        success: 'check-circle',
        error: 'exclamation-circle',
        warning: 'exclamation-triangle',
        info: 'info-circle'
    };
    return icons[type] || 'info-circle';
}

// Chart Initialization Helper
function initializeChart(canvasId, config) {
    const ctx = document.getElementById(canvasId).getContext('2d');
    return new Chart(ctx, config);
}

// Utility Functions
function formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
    }).format(amount);
}

function formatNumber(number) {
    return new Intl.NumberFormat('en-US').format(number);
}

// Export for use in other scripts
window.UI = {
    showNotification,
    initializeChart,
    formatCurrency,
    formatNumber
};