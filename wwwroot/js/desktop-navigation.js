// Desktop Navigation JavaScript - Enhanced navigation features
class DesktopNavigation {
    constructor() {
        this.navigationHistory = [];
        this.maxHistoryLength = 10;
        this.favoritePages = JSON.parse(localStorage.getItem('desktop-favorites') || '[]');
        this.recentPages = JSON.parse(localStorage.getItem('desktop-recent') || '[]');
        
        this.init();
    }

    init() {
        this.setupNavigationTracking();
        this.setupContextMenus();
        this.setupFavorites();
        this.setupBreadcrumbs();
        this.setupNavigationShortcuts();
        this.renderNavigationEnhancements();
    }

    setupNavigationTracking() {
        // Track page visits for recent pages
        this.trackCurrentPage();
        
        // Listen for navigation events
        window.addEventListener('beforeunload', () => {
            this.saveNavigationState();
        });

        // Track programmatic navigation
        const originalPushState = history.pushState;
        const originalReplaceState = history.replaceState;
        
        history.pushState = (...args) => {
            originalPushState.apply(history, args);
            this.trackCurrentPage();
        };
        
        history.replaceState = (...args) => {
            originalReplaceState.apply(history, args);
            this.trackCurrentPage();
        };
        
        window.addEventListener('popstate', () => {
            this.trackCurrentPage();
        });
    }

    trackCurrentPage() {
        const currentPage = {
            url: window.location.pathname,
            title: document.title.split(' - ')[0] || 'Page',
            timestamp: Date.now()
        };

        // Add to navigation history
        this.navigationHistory.unshift(currentPage);
        if (this.navigationHistory.length > this.maxHistoryLength) {
            this.navigationHistory = this.navigationHistory.slice(0, this.maxHistoryLength);
        }

        // Add to recent pages (avoid duplicates)
        this.recentPages = this.recentPages.filter(page => page.url !== currentPage.url);
        this.recentPages.unshift(currentPage);
        if (this.recentPages.length > 10) {
            this.recentPages = this.recentPages.slice(0, 10);
        }

        this.saveNavigationState();
    }

    saveNavigationState() {
        localStorage.setItem('desktop-recent', JSON.stringify(this.recentPages));
        localStorage.setItem('desktop-favorites', JSON.stringify(this.favoritePages));
    }

    setupContextMenus() {
        // Right-click context menu for navigation items
        document.addEventListener('contextmenu', (e) => {
            const navItem = e.target.closest('.desktop-nav-item');
            if (navItem) {
                e.preventDefault();
                this.showContextMenu(e, navItem);
            }
        });

        // Hide context menu on click elsewhere
        document.addEventListener('click', () => {
            this.hideContextMenu();
        });
    }

    showContextMenu(event, navItem) {
        this.hideContextMenu(); // Hide any existing menu
        
        const url = navItem.getAttribute('href');
        const title = navItem.textContent.trim();
        const isFavorite = this.favoritePages.some(fav => fav.url === url);

        const menu = document.createElement('div');
        menu.className = 'desktop-context-menu';
        menu.innerHTML = `
            <div class="desktop-context-item" onclick="navigation.openInNewTab('${url}')">
                <i class="fas fa-external-link-alt"></i>
                <span>Open in New Tab</span>
            </div>
            <div class="desktop-context-item" onclick="navigation.toggleFavorite('${url}', '${title}')">
                <i class="fas fa-${isFavorite ? 'heart-broken' : 'heart'}"></i>
                <span>${isFavorite ? 'Remove from' : 'Add to'} Favorites</span>
            </div>
            <div class="desktop-context-divider"></div>
            <div class="desktop-context-item" onclick="navigation.copyLink('${url}')">
                <i class="fas fa-copy"></i>
                <span>Copy Link</span>
            </div>
        `;

        // Position the menu
        menu.style.left = event.pageX + 'px';
        menu.style.top = event.pageY + 'px';

        document.body.appendChild(menu);

        // Ensure menu stays within viewport
        const rect = menu.getBoundingClientRect();
        if (rect.right > window.innerWidth) {
            menu.style.left = (event.pageX - rect.width) + 'px';
        }
        if (rect.bottom > window.innerHeight) {
            menu.style.top = (event.pageY - rect.height) + 'px';
        }
    }

    hideContextMenu() {
        const existingMenu = document.querySelector('.desktop-context-menu');
        if (existingMenu) {
            existingMenu.remove();
        }
    }

    openInNewTab(url) {
        window.open(url, '_blank');
        this.hideContextMenu();
    }

    toggleFavorite(url, title) {
        const existingIndex = this.favoritePages.findIndex(fav => fav.url === url);
        
        if (existingIndex > -1) {
            this.favoritePages.splice(existingIndex, 1);
            desktop.showNotification('Removed from favorites', 'info', 2000);
        } else {
            this.favoritePages.unshift({ url, title, timestamp: Date.now() });
            if (this.favoritePages.length > 20) {
                this.favoritePages = this.favoritePages.slice(0, 20);
            }
            desktop.showNotification('Added to favorites', 'success', 2000);
        }
        
        this.saveNavigationState();
        this.renderFavorites();
        this.hideContextMenu();
    }

    copyLink(url) {
        const fullUrl = window.location.origin + url;
        
        if (navigator.clipboard) {
            navigator.clipboard.writeText(fullUrl).then(() => {
                desktop.showNotification('Link copied to clipboard', 'success', 2000);
            });
        } else {
            // Fallback for older browsers
            const textArea = document.createElement('textarea');
            textArea.value = fullUrl;
            document.body.appendChild(textArea);
            textArea.select();
            document.execCommand('copy');
            document.body.removeChild(textArea);
            desktop.showNotification('Link copied to clipboard', 'success', 2000);
        }
        
        this.hideContextMenu();
    }

    setupFavorites() {
        this.renderFavorites();
    }

    renderFavorites() {
        const favoritesContainer = document.querySelector('.desktop-favorites-container');
        if (!favoritesContainer && this.favoritePages.length > 0) {
            this.createFavoritesSection();
        }

        this.updateFavoritesDisplay();
    }

    createFavoritesSection() {
        const sidebar = document.querySelector('.desktop-sidebar-nav');
        if (!sidebar) return;

        const favoritesSection = document.createElement('div');
        favoritesSection.className = 'desktop-nav-section desktop-favorites-section';
        favoritesSection.innerHTML = `
            <div class="desktop-nav-header">
                <i class="fas fa-heart"></i>
                <span>Favorites</span>
                <button class="desktop-nav-header-action" onclick="navigation.toggleFavoritesCollapse()" title="Toggle Favorites">
                    <i class="fas fa-chevron-down"></i>
                </button>
            </div>
            <div class="desktop-favorites-container"></div>
        `;

        // Insert after the first section (usually Dashboard)
        const firstSection = sidebar.querySelector('.desktop-nav-section');
        if (firstSection && firstSection.nextSibling) {
            sidebar.insertBefore(favoritesSection, firstSection.nextSibling);
        } else {
            sidebar.appendChild(favoritesSection);
        }
    }

    updateFavoritesDisplay() {
        const container = document.querySelector('.desktop-favorites-container');
        if (!container) return;

        if (this.favoritePages.length === 0) {
            container.innerHTML = `
                <div class="desktop-nav-empty">
                    <i class="fas fa-heart"></i>
                    <span>No favorites yet</span>
                </div>
            `;
        } else {
            container.innerHTML = this.favoritePages.map(favorite => `
                <a href="${favorite.url}" class="desktop-nav-item desktop-nav-favorite" data-url="${favorite.url}">
                    <i class="fas fa-star"></i>
                    <span>${favorite.title}</span>
                    <button class="desktop-nav-item-action" onclick="navigation.removeFavorite('${favorite.url}')" title="Remove Favorite">
                        <i class="fas fa-times"></i>
                    </button>
                </a>
            `).join('');
        }
    }

    removeFavorite(url) {
        this.favoritePages = this.favoritePages.filter(fav => fav.url !== url);
        this.saveNavigationState();
        this.renderFavorites();
        desktop.showNotification('Removed from favorites', 'info', 2000);
    }

    toggleFavoritesCollapse() {
        const section = document.querySelector('.desktop-favorites-section');
        const icon = section.querySelector('.desktop-nav-header-action i');
        
        section.classList.toggle('collapsed');
        icon.classList.toggle('fa-chevron-down');
        icon.classList.toggle('fa-chevron-right');
    }

    setupBreadcrumbs() {
        this.updateBreadcrumbs();
    }

    updateBreadcrumbs() {
        const breadcrumbContainer = document.querySelector('.desktop-breadcrumb');
        if (!breadcrumbContainer) return;

        const pathSegments = window.location.pathname.split('/').filter(segment => segment);
        const breadcrumbs = this.generateBreadcrumbs(pathSegments);

        breadcrumbContainer.innerHTML = breadcrumbs.map((crumb, index) => {
            if (index === breadcrumbs.length - 1) {
                return `<span class="desktop-breadcrumb-item current">${crumb.title}</span>`;
            } else {
                return `
                    <a href="${crumb.url}" class="desktop-breadcrumb-item">
                        ${crumb.title}
                    </a>
                    <i class="fas fa-chevron-right desktop-breadcrumb-separator"></i>
                `;
            }
        }).join('');
    }

    generateBreadcrumbs(pathSegments) {
        const breadcrumbs = [{ title: 'Home', url: '/' }];
        
        let currentPath = '';
        const segmentTitles = {
            'companies': 'Companies',
            'deals': 'Deals',
            'products': 'Products',
            'oems': 'OEM Partners',
            'projectpipeline': 'Pipeline',
            'dashboard': 'Dashboard',
            'reports': 'Reports',
            'usermanagement': 'User Management',
            'create': 'Create',
            'edit': 'Edit',
            'details': 'Details'
        };

        pathSegments.forEach((segment, index) => {
            currentPath += '/' + segment;
            const title = segmentTitles[segment.toLowerCase()] || 
                         this.capitalizeFirst(segment.replace(/[-_]/g, ' '));
            
            breadcrumbs.push({
                title,
                url: currentPath
            });
        });

        return breadcrumbs;
    }

    setupNavigationShortcuts() {
        document.addEventListener('keydown', (e) => {
            // Alt + number keys for quick navigation
            if (e.altKey && e.key >= '1' && e.key <= '9') {
                e.preventDefault();
                this.navigateToQuickAccess(parseInt(e.key) - 1);
            }

            // Ctrl/Cmd + Shift + F for favorites
            if ((e.ctrlKey || e.metaKey) && e.shiftKey && e.key === 'F') {
                e.preventDefault();
                this.showFavoritesQuickAccess();
            }

            // Ctrl/Cmd + Shift + H for history
            if ((e.ctrlKey || e.metaKey) && e.shiftKey && e.key === 'H') {
                e.preventDefault();
                this.showHistoryQuickAccess();
            }
        });
    }

    navigateToQuickAccess(index) {
        const quickLinks = [
            '/',
            '/Companies',
            '/Deals',
            '/Products',
            '/Oems',
            '/ProjectPipeline',
            '/Dashboard',
            '/Reports'
        ];

        if (index < quickLinks.length) {
            window.location.href = quickLinks[index];
        }
    }

    showFavoritesQuickAccess() {
        if (this.favoritePages.length === 0) {
            desktop.showNotification('No favorites saved', 'info', 2000);
            return;
        }

        // Show favorites in a quick access modal
        this.showQuickAccessModal('Favorites', this.favoritePages);
    }

    showHistoryQuickAccess() {
        if (this.navigationHistory.length === 0) {
            desktop.showNotification('No navigation history', 'info', 2000);
            return;
        }

        // Show recent pages in a quick access modal
        this.showQuickAccessModal('Recent Pages', this.navigationHistory);
    }

    showQuickAccessModal(title, items) {
        // Remove existing modal
        const existingModal = document.getElementById('desktop-quick-access-modal');
        if (existingModal) {
            existingModal.remove();
        }

        const modal = document.createElement('div');
        modal.id = 'desktop-quick-access-modal';
        modal.className = 'desktop-quick-access-modal';
        modal.innerHTML = `
            <div class="desktop-quick-access-content">
                <div class="desktop-quick-access-header">
                    <h3>${title}</h3>
                    <button class="desktop-quick-access-close" onclick="this.closest('.desktop-quick-access-modal').remove()">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <div class="desktop-quick-access-body">
                    ${items.map((item, index) => `
                        <a href="${item.url}" class="desktop-quick-access-item" data-index="${index}">
                            <div class="desktop-quick-access-item-main">
                                <span class="desktop-quick-access-item-title">${item.title}</span>
                                <span class="desktop-quick-access-item-url">${item.url}</span>
                            </div>
                            <div class="desktop-quick-access-item-meta">
                                ${item.timestamp ? this.formatTimestamp(item.timestamp) : ''}
                            </div>
                        </a>
                    `).join('')}
                </div>
                <div class="desktop-quick-access-footer">
                    <span class="desktop-quick-access-hint">
                        Use ↑↓ arrow keys to navigate, Enter to select, Esc to close
                    </span>
                </div>
            </div>
        `;

        document.body.appendChild(modal);

        // Setup keyboard navigation
        let selectedIndex = 0;
        const items = modal.querySelectorAll('.desktop-quick-access-item');
        
        const updateSelection = () => {
            items.forEach((item, index) => {
                item.classList.toggle('selected', index === selectedIndex);
            });
        };

        updateSelection();

        const keyHandler = (e) => {
            switch(e.key) {
                case 'ArrowDown':
                    e.preventDefault();
                    selectedIndex = (selectedIndex + 1) % items.length;
                    updateSelection();
                    break;
                case 'ArrowUp':
                    e.preventDefault();
                    selectedIndex = selectedIndex === 0 ? items.length - 1 : selectedIndex - 1;
                    updateSelection();
                    break;
                case 'Enter':
                    e.preventDefault();
                    items[selectedIndex].click();
                    break;
                case 'Escape':
                    e.preventDefault();
                    modal.remove();
                    break;
            }
        };

        document.addEventListener('keydown', keyHandler);

        // Cleanup on modal close
        modal.addEventListener('remove', () => {
            document.removeEventListener('keydown', keyHandler);
        });

        // Focus the modal for keyboard events
        modal.focus();
    }

    renderNavigationEnhancements() {
        // Add navigation enhancement indicators
        this.addActivePageIndicators();
        this.addNavigationTooltips();
    }

    addActivePageIndicators() {
        const currentPath = window.location.pathname;
        const navItems = document.querySelectorAll('.desktop-nav-item');

        navItems.forEach(item => {
            const href = item.getAttribute('href');
            if (href) {
                if (currentPath === href || 
                    (href !== '/' && currentPath.startsWith(href))) {
                    item.classList.add('active');
                }
            }
        });
    }

    addNavigationTooltips() {
        const navItems = document.querySelectorAll('.desktop-nav-item');
        
        navItems.forEach(item => {
            const text = item.textContent.trim();
            if (text && !item.hasAttribute('title')) {
                item.setAttribute('title', text);
            }
        });
    }

    // Utility methods
    capitalizeFirst(str) {
        return str.charAt(0).toUpperCase() + str.slice(1);
    }

    formatTimestamp(timestamp) {
        const date = new Date(timestamp);
        const now = new Date();
        const diffMs = now - date;
        const diffMinutes = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMinutes / 60);
        const diffDays = Math.floor(diffHours / 24);

        if (diffMinutes < 1) return 'Just now';
        if (diffMinutes < 60) return `${diffMinutes}m ago`;
        if (diffHours < 24) return `${diffHours}h ago`;
        if (diffDays < 7) return `${diffDays}d ago`;
        return date.toLocaleDateString();
    }
}

// Initialize navigation enhancement
const navigation = new DesktopNavigation();

// Export for modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DesktopNavigation;
}
