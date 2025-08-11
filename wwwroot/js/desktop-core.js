// Desktop Core JavaScript - Main framework functions
class DesktopFramework {
    constructor() {
        this.sidebarCollapsed = localStorage.getItem('sidebar-collapsed') === 'true';
        this.init();
    }

    init() {
        this.setupEventListeners();
        this.setupKeyboardShortcuts();
        this.setupSidebar();
        this.setupNotifications();
        this.setupSearch();
        this.setupTheme();
        
        // Initialize on DOM ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => this.onReady());
        } else {
            this.onReady();
        }
    }

    onReady() {
        this.applySidebarState();
        this.updateBreadcrumb();
        this.initializeTooltips();
        this.setupResponsiveLayout();
    }

    setupEventListeners() {
        // Sidebar toggle
        document.addEventListener('click', (e) => {
            if (e.target.closest('.desktop-sidebar-toggle')) {
                e.preventDefault();
                this.toggleSidebar();
            }
        });

        // Search functionality
        document.addEventListener('input', (e) => {
            if (e.target.classList.contains('desktop-search-input')) {
                this.handleSearch(e.target.value);
            }
        });

        // Quick actions
        document.addEventListener('click', (e) => {
            if (e.target.closest('.desktop-quick-btn')) {
                e.preventDefault();
                this.showQuickActions();
            }
        });

        // Notifications
        document.addEventListener('click', (e) => {
            if (e.target.closest('.desktop-notification-btn')) {
                e.preventDefault();
                this.toggleNotifications();
            }
        });

        // Window resize
        window.addEventListener('resize', () => {
            this.handleResize();
        });

        // Theme toggle
        document.addEventListener('click', (e) => {
            if (e.target.closest('.desktop-theme-toggle')) {
                e.preventDefault();
                this.toggleTheme();
            }
        });
    }

    setupKeyboardShortcuts() {
        document.addEventListener('keydown', (e) => {
            // Ctrl/Cmd + K for search
            if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                this.focusSearch();
            }

            // Ctrl/Cmd + B for sidebar toggle
            if ((e.ctrlKey || e.metaKey) && e.key === 'b') {
                e.preventDefault();
                this.toggleSidebar();
            }

            // Ctrl/Cmd + N for new (context-aware)
            if ((e.ctrlKey || e.metaKey) && e.key === 'n') {
                e.preventDefault();
                this.createNew();
            }

            // ESC to close modals/dropdowns
            if (e.key === 'Escape') {
                this.closeModals();
            }

            // F11 for fullscreen
            if (e.key === 'F11') {
                e.preventDefault();
                this.toggleFullscreen();
            }
        });
    }

    setupSidebar() {
        const sidebar = document.querySelector('.desktop-sidebar');
        if (!sidebar) return;

        // Collapse/expand animation
        sidebar.style.transition = 'width 0.3s ease, transform 0.3s ease';

        // Active page highlighting
        this.highlightActivePage();

        // Hover effects for collapsed sidebar
        if (this.sidebarCollapsed) {
            sidebar.addEventListener('mouseenter', () => {
                if (this.sidebarCollapsed) {
                    sidebar.classList.add('hover-expanded');
                }
            });

            sidebar.addEventListener('mouseleave', () => {
                sidebar.classList.remove('hover-expanded');
            });
        }
    }

    toggleSidebar() {
        const container = document.querySelector('.desktop-container');
        const sidebar = document.querySelector('.desktop-sidebar');
        
        if (!container || !sidebar) return;

        this.sidebarCollapsed = !this.sidebarCollapsed;
        
        if (this.sidebarCollapsed) {
            container.classList.add('sidebar-collapsed');
        } else {
            container.classList.remove('sidebar-collapsed');
        }

        localStorage.setItem('sidebar-collapsed', this.sidebarCollapsed.toString());
        
        // Dispatch custom event for other components to listen to
        document.dispatchEvent(new CustomEvent('sidebarToggled', {
            detail: { collapsed: this.sidebarCollapsed }
        }));
    }

    applySidebarState() {
        const container = document.querySelector('.desktop-container');
        if (!container) return;

        if (this.sidebarCollapsed) {
            container.classList.add('sidebar-collapsed');
        }
    }

    highlightActivePage() {
        const currentPath = window.location.pathname;
        const navItems = document.querySelectorAll('.desktop-nav-item');

        navItems.forEach(item => {
            const href = item.getAttribute('href');
            if (href && (currentPath === href || currentPath.startsWith(href + '/'))) {
                item.classList.add('active');
                
                // Expand parent section if needed
                const section = item.closest('.desktop-nav-section');
                if (section) {
                    section.classList.add('expanded');
                }
            }
        });
    }

    setupNotifications() {
        this.notificationQueue = [];
        this.notificationContainer = this.createNotificationContainer();
    }

    createNotificationContainer() {
        let container = document.getElementById('desktop-notifications');
        if (!container) {
            container = document.createElement('div');
            container.id = 'desktop-notifications';
            container.className = 'desktop-notifications-container';
            document.body.appendChild(container);
        }
        return container;
    }

    showNotification(message, type = 'info', duration = 5000) {
        const notification = document.createElement('div');
        notification.className = `desktop-notification desktop-notification-${type}`;
        
        const icon = this.getNotificationIcon(type);
        notification.innerHTML = `
            <div class="desktop-notification-content">
                <i class="${icon}"></i>
                <span>${message}</span>
            </div>
            <button class="desktop-notification-close">
                <i class="fas fa-times"></i>
            </button>
        `;

        // Add click to close
        notification.querySelector('.desktop-notification-close').addEventListener('click', () => {
            this.removeNotification(notification);
        });

        // Auto remove after duration
        if (duration > 0) {
            setTimeout(() => {
                this.removeNotification(notification);
            }, duration);
        }

        this.notificationContainer.appendChild(notification);
        
        // Trigger animation
        requestAnimationFrame(() => {
            notification.classList.add('show');
        });

        return notification;
    }

    removeNotification(notification) {
        notification.classList.remove('show');
        notification.classList.add('hide');
        
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 300);
    }

    getNotificationIcon(type) {
        const icons = {
            success: 'fas fa-check-circle',
            error: 'fas fa-exclamation-circle',
            warning: 'fas fa-exclamation-triangle',
            info: 'fas fa-info-circle'
        };
        return icons[type] || icons.info;
    }

    setupSearch() {
        this.searchResults = [];
        this.searchIndex = this.buildSearchIndex();
    }

    buildSearchIndex() {
        // Build search index from navigation items and page content
        const index = [];
        
        // Add navigation items
        document.querySelectorAll('.desktop-nav-item').forEach(item => {
            const text = item.textContent.trim();
            const href = item.getAttribute('href');
            if (text && href) {
                index.push({
                    type: 'navigation',
                    title: text,
                    url: href,
                    searchText: text.toLowerCase()
                });
            }
        });

        return index;
    }

    handleSearch(query) {
        if (!query.trim()) {
            this.hideSearchResults();
            return;
        }

        const results = this.searchIndex.filter(item => 
            item.searchText.includes(query.toLowerCase())
        ).slice(0, 10);

        this.showSearchResults(results, query);
    }

    showSearchResults(results, query) {
        let resultsContainer = document.querySelector('.desktop-search-results');
        if (!resultsContainer) {
            resultsContainer = document.createElement('div');
            resultsContainer.className = 'desktop-search-results';
            const searchContainer = document.querySelector('.desktop-search');
            searchContainer.appendChild(resultsContainer);
        }

        if (results.length === 0) {
            resultsContainer.innerHTML = `
                <div class="desktop-search-empty">
                    <i class="fas fa-search"></i>
                    <span>No results found for "${query}"</span>
                </div>
            `;
        } else {
            resultsContainer.innerHTML = results.map(result => `
                <a href="${result.url}" class="desktop-search-result">
                    <i class="fas fa-${result.type === 'navigation' ? 'link' : 'file-alt'}"></i>
                    <span>${this.highlightMatch(result.title, query)}</span>
                </a>
            `).join('');
        }

        resultsContainer.style.display = 'block';
    }

    hideSearchResults() {
        const resultsContainer = document.querySelector('.desktop-search-results');
        if (resultsContainer) {
            resultsContainer.style.display = 'none';
        }
    }

    highlightMatch(text, query) {
        const regex = new RegExp(`(${query})`, 'gi');
        return text.replace(regex, '<mark>$1</mark>');
    }

    focusSearch() {
        const searchInput = document.querySelector('.desktop-search-input');
        if (searchInput) {
            searchInput.focus();
            searchInput.select();
        }
    }

    setupTheme() {
        this.currentTheme = localStorage.getItem('desktop-theme') || 'light';
        this.applyTheme(this.currentTheme);
    }

    toggleTheme() {
        this.currentTheme = this.currentTheme === 'light' ? 'dark' : 'light';
        this.applyTheme(this.currentTheme);
        localStorage.setItem('desktop-theme', this.currentTheme);
    }

    applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        
        // Update theme toggle icon
        const themeToggle = document.querySelector('.desktop-theme-toggle i');
        if (themeToggle) {
            themeToggle.className = theme === 'light' ? 'fas fa-moon' : 'fas fa-sun';
        }
    }

    showQuickActions() {
        // Show quick action modal or dropdown
        this.showNotification('Quick actions feature coming soon!', 'info');
    }

    toggleNotifications() {
        // Toggle notification panel
        this.showNotification('Notification panel feature coming soon!', 'info');
    }

    createNew() {
        // Context-aware new item creation
        const currentController = this.getCurrentController();
        const actions = {
            companies: '/Companies/Create',
            deals: '/Deals/Create',
            products: '/Products/Create',
            oems: '/Oems/Create'
        };

        const url = actions[currentController] || '/Companies/Create';
        window.location.href = url;
    }

    getCurrentController() {
        const path = window.location.pathname.toLowerCase();
        if (path.includes('/companies')) return 'companies';
        if (path.includes('/deals')) return 'deals';
        if (path.includes('/products')) return 'products';
        if (path.includes('/oems')) return 'oems';
        return 'home';
    }

    updateBreadcrumb() {
        const breadcrumb = document.querySelector('.desktop-breadcrumb-item');
        if (breadcrumb && !breadcrumb.textContent.trim()) {
            breadcrumb.textContent = document.title.split(' - ')[0] || 'Dashboard';
        }
    }

    initializeTooltips() {
        // Initialize Bootstrap tooltips for desktop elements
        const tooltipElements = document.querySelectorAll('[title]');
        tooltipElements.forEach(element => {
            if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
                new bootstrap.Tooltip(element);
            }
        });
    }

    setupResponsiveLayout() {
        this.handleResize();
    }

    handleResize() {
        const width = window.innerWidth;
        const container = document.querySelector('.desktop-container');
        
        if (!container) return;

        // Auto-collapse sidebar on smaller screens
        if (width < 1200 && !this.sidebarCollapsed) {
            this.toggleSidebar();
        }

        // Update responsive classes
        container.classList.toggle('desktop-mobile', width < 768);
        container.classList.toggle('desktop-tablet', width >= 768 && width < 1200);
        container.classList.toggle('desktop-large', width >= 1200);
    }

    closeModals() {
        // Close any open modals or dropdowns
        const modals = document.querySelectorAll('.modal.show');
        modals.forEach(modal => {
            const modalInstance = bootstrap.Modal.getInstance(modal);
            if (modalInstance) {
                modalInstance.hide();
            }
        });

        // Close dropdowns
        const dropdowns = document.querySelectorAll('.dropdown-menu.show');
        dropdowns.forEach(dropdown => {
            dropdown.classList.remove('show');
        });

        // Hide search results
        this.hideSearchResults();
    }

    toggleFullscreen() {
        if (!document.fullscreenElement) {
            document.documentElement.requestFullscreen();
        } else {
            document.exitFullscreen();
        }
    }

    // Utility methods
    debounce(func, wait) {
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

    throttle(func, limit) {
        let inThrottle;
        return function() {
            const args = arguments;
            const context = this;
            if (!inThrottle) {
                func.apply(context, args);
                inThrottle = true;
                setTimeout(() => inThrottle = false, limit);
            }
        }
    }
}

// Initialize desktop framework
const desktop = new DesktopFramework();

// Global functions for compatibility
function toggleSidebar() {
    desktop.toggleSidebar();
}

function showQuickActions() {
    desktop.showQuickActions();
}

function filterDealsByPhase(phase) {
    // Navigate to deals page with phase filter
    window.location.href = `/Deals/Index?phase=${phase}`;
}

function showContactsModal() {
    // Show contacts modal - placeholder
    desktop.showNotification('Contacts modal feature coming soon!', 'info');
}

// Export for modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DesktopFramework;
}
