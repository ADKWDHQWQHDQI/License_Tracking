// CBMS Activity Management - Enhanced Desktop JavaScript
// Bigin.com-inspired functionality for Activity Management

// Global variables
let activityFiltersExpanded = true;
let currentSort = 'dueDate-asc';
let selectedActivities = [];

// Initialize when DOM is ready
$(document).ready(function() {
    initializeActivityManagement();
    setupKeyboardShortcuts();
    initializeTooltips();
    loadSavedFilterState();
    setupEnhancedFilters();
    initializeRealTimeUpdates();
});

// Core Activity Management Initialization
function initializeActivityManagement() {
    console.log('Initializing CBMS Activity Management...');
    
    // Setup enhanced table interactions
    setupTableInteractions();
    
    // Initialize filter collapse state
    loadFilterState();
    
    // Setup live search
    setupLiveSearch();
    
    // Initialize bulk selection
    setupBulkSelection();
    
    console.log('Activity Management initialized successfully');
}

// Enhanced Table Interactions
function setupTableInteractions() {
    // Row hover effects
    $('.activity-row').hover(
        function() {
            $(this).addClass('table-row-hover');
        },
        function() {
            $(this).removeClass('table-row-hover');
        }
    );
    
    // Sortable headers
    $('.sortable').click(function(e) {
        e.preventDefault();
        const sortField = $(this).data('sort');
        handleSort(sortField);
    });
    
    // Row click navigation (excluding action buttons)
    $('.activity-row').click(function(e) {
        if (!$(e.target).closest('.actions-cell, .select-cell, .bigin-checkbox').length) {
            const activityId = $(this).data('activity-id');
            if (activityId) {
                window.location.href = `/Activity/Details/${activityId}`;
            }
        }
    });
}

// Enhanced Keyboard Shortcuts
function setupKeyboardShortcuts() {
    $(document).keydown(function(e) {
        // Only process if not in input field
        if ($(e.target).is('input, textarea, select')) return;
        
        if (e.ctrlKey || e.metaKey) {
            switch(e.which) {
                case 78: // Ctrl+N - New Activity
                    e.preventDefault();
                    window.location.href = '/Activity/Create';
                    break;
                case 70: // Ctrl+F - Focus Search
                    e.preventDefault();
                    $('#globalSearchFilter').focus().select();
                    break;
                case 82: // Ctrl+R - Refresh
                    e.preventDefault();
                    refreshActivityView();
                    break;
                case 65: // Ctrl+A - Select All
                    e.preventDefault();
                    selectAllActivities();
                    break;
                case 88: // Ctrl+X - Export
                    e.preventDefault();
                    exportActivities();
                    break;
            }
        } else {
            switch(e.which) {
                case 27: // Escape - Clear selection
                    clearSelection();
                    break;
                case 46: // Delete - Bulk delete (if implemented)
                    if (selectedActivities.length > 0) {
                        bulkActionModal();
                    }
                    break;
            }
        }
    });
}

// Live Search Implementation
function setupLiveSearch() {
    const searchInput = $('#globalSearchFilter');
    let searchTimeout;
    
    searchInput.on('input', function() {
        clearTimeout(searchTimeout);
        const searchTerm = $(this).val().toLowerCase();
        
        searchTimeout = setTimeout(() => {
            performLiveSearch(searchTerm);
        }, 300);
    });
}

function performLiveSearch(searchTerm = '') {
    if (!searchTerm) {
        searchTerm = $('#globalSearchFilter').val().toLowerCase();
    }
    
    $('.activity-row').each(function() {
        const rowText = $(this).text().toLowerCase();
        const matches = searchTerm === '' || rowText.includes(searchTerm);
        $(this).toggle(matches);
    });
    
    updateRowCount();
}

// Bulk Selection Management
function setupBulkSelection() {
    // Master checkbox
    $('#selectAllCheckbox').change(function() {
        const isChecked = $(this).prop('checked');
        $('.activity-checkbox').prop('checked', isChecked);
        updateSelectedActivities();
    });
    
    // Individual checkboxes
    $(document).on('change', '.activity-checkbox', function() {
        updateSelectedActivities();
        updateMasterCheckbox();
    });
}

function updateSelectedActivities() {
    selectedActivities = $('.activity-checkbox:checked').map(function() {
        return $(this).val();
    }).get();
    
    updateBulkActionButtons();
}

function updateMasterCheckbox() {
    const totalCheckboxes = $('.activity-checkbox').length;
    const checkedCheckboxes = $('.activity-checkbox:checked').length;
    
    $('#selectAllCheckbox').prop('indeterminate', checkedCheckboxes > 0 && checkedCheckboxes < totalCheckboxes);
    $('#selectAllCheckbox').prop('checked', checkedCheckboxes === totalCheckboxes && totalCheckboxes > 0);
}

function updateBulkActionButtons() {
    const hasSelection = selectedActivities.length > 0;
    $('.bulk-action-btn').prop('disabled', !hasSelection);
    
    if (hasSelection) {
        $('#selectedCount').text(selectedActivities.length);
        $('.selection-info').show();
    } else {
        $('.selection-info').hide();
    }
}

// Enhanced Filter Management
function setupEnhancedFilters() {
    // Custom date range toggle
    $('#dateRangeFilter').change(function() {
        toggleCustomDateRange();
    });
    
    // Filter form auto-submit with debounce
    $('.bigin-select, .bigin-input').not('#globalSearchFilter').on('change', debounce(function() {
        if ($(this).closest('#activityFilterForm').length) {
            submitFilters();
        }
    }, 1000));
}

function toggleCustomDateRange() {
    const dateRange = $('#dateRangeFilter').val();
    $('#customDateRange').toggle(dateRange === 'custom');
}

function submitFilters() {
    const formData = $('#activityFilterForm').serialize();
    
    // Show loading state
    showLoadingState(true);
    
    // Submit form via AJAX or redirect
    window.location.href = `/Activity?${formData}`;
}

// Filter State Management
function loadFilterState() {
    const filtersExpanded = localStorage.getItem('activityFiltersExpanded') !== 'false';
    
    if (!filtersExpanded) {
        $('#activityFilterContent').hide();
        $('.filter-collapse-icon').removeClass('fa-chevron-down').addClass('fa-chevron-up');
    }
}

function toggleFilterCollapse() {
    const content = $('#activityFilterContent');
    const icon = $('.filter-collapse-icon');
    
    content.slideToggle(300, function() {
        const isVisible = content.is(':visible');
        icon.toggleClass('fa-chevron-down fa-chevron-up');
        localStorage.setItem('activityFiltersExpanded', isVisible);
    });
}

// Sorting Implementation
function handleSort(field) {
    const currentSortParts = currentSort.split('-');
    const currentField = currentSortParts[0];
    const currentDirection = currentSortParts[1];
    
    let newDirection = 'asc';
    if (currentField === field && currentDirection === 'asc') {
        newDirection = 'desc';
    }
    
    currentSort = `${field}-${newDirection}`;
    localStorage.setItem('activitySort', currentSort);
    
    sortActivitiesTable(field, newDirection === 'desc');
}

function sortActivitiesTable(field, descending) {
    const tbody = $('#activitiesTable tbody');
    const rows = tbody.find('tr').get();
    
    rows.sort(function(a, b) {
        let aVal = $(a).find(`[data-sort-value]`).attr('data-sort-value') || 
                   $(a).find(`td:eq(${getColumnIndex(field)})`).text().trim();
        let bVal = $(b).find(`[data-sort-value]`).attr('data-sort-value') || 
                   $(b).find(`td:eq(${getColumnIndex(field)})`).text().trim();
        
        // Handle different data types
        if (field === 'dueDate') {
            aVal = new Date(aVal);
            bVal = new Date(bVal);
        } else if (field === 'priority') {
            const priorityOrder = { 'Low': 1, 'Medium': 2, 'High': 3, 'Urgent': 4 };
            aVal = priorityOrder[aVal] || 0;
            bVal = priorityOrder[bVal] || 0;
        }
        
        if (descending) {
            return aVal < bVal ? 1 : -1;
        } else {
            return aVal > bVal ? 1 : -1;
        }
    });
    
    // Reorder rows
    $.each(rows, function(index, row) {
        tbody.append(row);
    });
    
    // Update sort icons
    updateSortIcons(field, descending);
}

function getColumnIndex(field) {
    const columnMap = {
        'subject': 1,
        'type': 2,
        'entity': 3,
        'dueDate': 4,
        'status': 5,
        'priority': 6,
        'assignedTo': 7
    };
    return columnMap[field] || 0;
}

function updateSortIcons(field, descending) {
    $('.sort-icon').removeClass('fa-sort-up fa-sort-down').addClass('fa-sort');
    $(`.sortable[data-sort="${field}"] .sort-icon`)
        .removeClass('fa-sort')
        .addClass(descending ? 'fa-sort-down' : 'fa-sort-up');
}

// Activity Actions
function markAsCompleted(activityId) {
    if (!confirm('Mark this activity as completed?')) return;
    
    showLoadingState(true);
    
    $.ajax({
        url: '/Activity/MarkCompleted',
        type: 'POST',
        data: { 
            id: activityId,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                showNotification('Activity marked as completed', 'success');
                refreshActivityView();
            } else {
                showNotification(response.message || 'Error updating activity', 'error');
            }
        },
        error: function() {
            showNotification('Error updating activity', 'error');
        },
        complete: function() {
            showLoadingState(false);
        }
    });
}

function followUpActivity(activityId) {
    window.location.href = `/Activity/Create?followUpTo=${activityId}`;
}

// Bulk Actions
function bulkActionModal() {
    if (selectedActivities.length === 0) {
        showNotification('Please select activities first', 'warning');
        return;
    }
    
    const actions = [
        { id: 'markCompleted', label: 'Mark as Completed', icon: 'fa-check', class: 'btn-success' },
        { id: 'markPending', label: 'Mark as Pending', icon: 'fa-clock', class: 'btn-warning' },
        { id: 'reassign', label: 'Reassign', icon: 'fa-user-plus', class: 'btn-info' },
        { id: 'delete', label: 'Delete', icon: 'fa-trash', class: 'btn-danger' }
    ];
    
    let modalHtml = `
        <div class="modal fade" id="bulkActionModal" tabindex="-1">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">
                            <i class="fas fa-tasks"></i>
                            Bulk Actions (${selectedActivities.length} selected)
                        </h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <p>Choose an action to apply to the selected activities:</p>
                        <div class="d-grid gap-2">`;
    
    actions.forEach(action => {
        modalHtml += `
            <button type="button" class="btn ${action.class}" onclick="performBulkAction('${action.id}')">
                <i class="fas ${action.icon}"></i>
                ${action.label}
            </button>`;
    });
    
    modalHtml += `
                        </div>
                    </div>
                </div>
            </div>
        </div>`;
    
    // Remove existing modal
    $('#bulkActionModal').remove();
    
    // Add and show new modal
    $('body').append(modalHtml);
    $('#bulkActionModal').modal('show');
}

function performBulkAction(action) {
    $('#bulkActionModal').modal('hide');
    
    if (selectedActivities.length === 0) return;
    
    showLoadingState(true);
    
    $.ajax({
        url: '/Activity/BulkAction',
        type: 'POST',
        data: {
            action: action,
            activityIds: selectedActivities,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                showNotification(`Bulk action completed: ${response.message}`, 'success');
                refreshActivityView();
            } else {
                showNotification(response.message || 'Error performing bulk action', 'error');
            }
        },
        error: function() {
            showNotification('Error performing bulk action', 'error');
        },
        complete: function() {
            showLoadingState(false);
        }
    });
}

// Export Functionality
function exportActivities() {
    const filterData = $('#activityFilterForm').serialize();
    const selectedData = selectedActivities.length > 0 ? `&selectedIds=${selectedActivities.join(',')}` : '';
    
    showNotification('Preparing export...', 'info');
    
    // Create download link
    const downloadUrl = `/Activity/Export?${filterData}${selectedData}`;
    
    // Use hidden iframe for download
    const iframe = $('<iframe>').hide().appendTo('body');
    iframe.attr('src', downloadUrl);
    
    // Clean up after download
    setTimeout(() => {
        iframe.remove();
        showNotification('Export completed', 'success');
    }, 3000);
}

// View Management
function refreshActivityView(silent = false) {
    if (!silent) {
        showNotification('Refreshing activities...', 'info');
    }
    
    // Preserve current filter and sort state
    const currentUrl = new URL(window.location);
    window.location.reload();
}

function toggleActivityViewMode() {
    const currentView = localStorage.getItem('activityViewMode') || 'list';
    const newView = currentView === 'list' ? 'card' : 'list';
    
    localStorage.setItem('activityViewMode', newView);
    showNotification(`Switched to ${newView} view`, 'info');
    
    // Implement view toggle logic here
    if (newView === 'card') {
        switchToCardView();
    } else {
        switchToListView();
    }
}

function switchToCardView() {
    // Implementation for card view
    console.log('Switching to card view...');
}

function switchToListView() {
    // Implementation for list view
    console.log('Switching to list view...');
}

// Utility Functions
function clearSelection() {
    $('.activity-checkbox').prop('checked', false);
    $('#selectAllCheckbox').prop('checked', false);
    updateSelectedActivities();
}

function updateRowCount() {
    const visibleRows = $('.activity-row:visible').length;
    const totalRows = $('.activity-row').length;
    
    $('.panel-count').text(`${visibleRows} of ${totalRows} activities`);
}

function showLoadingState(show) {
    if (show) {
        $('body').addClass('loading');
        $('.bigin-table-container').addClass('loading-overlay');
    } else {
        $('body').removeClass('loading');
        $('.bigin-table-container').removeClass('loading-overlay');
    }
}

function showNotification(message, type = 'info') {
    // Use browser notification API or toast library
    console.log(`${type.toUpperCase()}: ${message}`);
    
    // Create toast notification
    const toastHtml = `
        <div class="toast align-items-center text-white bg-${type === 'error' ? 'danger' : type === 'success' ? 'success' : type === 'warning' ? 'warning' : 'primary'} border-0" role="alert">
            <div class="d-flex">
                <div class="toast-body">
                    <i class="fas fa-${type === 'error' ? 'exclamation-triangle' : type === 'success' ? 'check' : type === 'warning' ? 'exclamation' : 'info'}"></i>
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>`;
    
    // Add toast to container
    if (!$('.toast-container').length) {
        $('body').append('<div class="toast-container position-fixed top-0 end-0 p-3"></div>');
    }
    
    const $toast = $(toastHtml);
    $('.toast-container').append($toast);
    
    // Show toast
    const toast = new bootstrap.Toast($toast[0]);
    toast.show();
    
    // Auto-remove after 5 seconds
    setTimeout(() => {
        $toast.remove();
    }, 5000);
}

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

function initializeTooltips() {
    // Initialize Bootstrap tooltips
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        $('[title]').each(function() {
            new bootstrap.Tooltip(this, {
                placement: 'top',
                trigger: 'hover'
            });
        });
    }
}

// Real-time Updates (if WebSocket is available)
function initializeRealTimeUpdates() {
    // Placeholder for real-time updates
    // This would connect to SignalR or WebSocket for live updates
    console.log('Real-time updates initialized');
}

// Initialize saved filter state
function loadSavedFilterState() {
    const savedSort = localStorage.getItem('activitySort');
    if (savedSort) {
        currentSort = savedSort;
        const [field, direction] = savedSort.split('-');
        updateSortIcons(field, direction === 'desc');
    }
}

// Export functions for global access
window.ActivityManagement = {
    markAsCompleted,
    followUpActivity,
    bulkActionModal,
    exportActivities,
    refreshActivityView,
    toggleActivityViewMode,
    toggleFilterCollapse,
    clearAllFilters,
    saveCurrentFilter,
    applyQuickFilter,
    resetFilters
};
