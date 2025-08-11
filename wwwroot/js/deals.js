// Deal Management JavaScript for Professional CBMS Enterprise Application
// Desktop-first design with Bigin.com style interactions
// Author: CBMS Development Team
// License: Enterprise CBMS License Tracking System

$(document).ready(function() {
    // Initialize Deal Management System
    initializeDealSystem();
    
    // Initialize form validation and interactions
    initializeFormValidation();
    
    // Initialize dynamic dropdowns
    initializeDynamicDropdowns();
    
    // Initialize KPI dashboard
    initializeKPIDashboard();
    
    // Initialize advanced filtering
    initializeAdvancedFiltering();
});

// ================================================
// CORE DEAL SYSTEM INITIALIZATION
// ================================================

function initializeDealSystem() {
    console.log('CBMS Deal Management System - Enterprise Edition Initialized');
    
    // Initialize tooltips for professional UI
    $('[data-toggle="tooltip"]').tooltip();
    
    // Initialize professional notifications
    initializeNotifications();
    
    // Initialize responsive table behaviors
    initializeResponsiveTables();
    
    // Initialize professional animations
    initializeProfessionalAnimations();
}

// ================================================
// FORM VALIDATION SYSTEM
// ================================================

function initializeFormValidation() {
    // Real-time form validation for professional UX
    $('.deal-form input[required], .deal-form select[required]').on('blur', function() {
        validateField($(this));
    });
    
    // Professional form submission handling
    $('.deal-form').on('submit', function(e) {
        if (!validateDealForm($(this))) {
            e.preventDefault();
            showValidationErrors();
        }
    });
    
    // Real-time calculation updates
    $('#Quantity, #UnitPrice, #CustomerInvoiceAmount, #OemQuoteAmount').on('input', function() {
        calculateDealFinancials();
    });
}

function validateField(field) {
    const fieldName = field.attr('name');
    const value = field.val();
    const fieldGroup = field.closest('.form-group');
    
    // Remove existing validation states
    fieldGroup.removeClass('has-error has-success');
    fieldGroup.find('.validation-message').remove();
    
    // Apply professional validation styling
    if (field.prop('required') && (!value || value.trim() === '')) {
        fieldGroup.addClass('has-error');
        field.after('<div class="validation-message error">This field is required</div>');
        return false;
    } else {
        fieldGroup.addClass('has-success');
        return true;
    }
}

function validateDealForm(form) {
    let isValid = true;
    const requiredFields = form.find('[required]');
    
    requiredFields.each(function() {
        if (!validateField($(this))) {
            isValid = false;
        }
    });
    
    // Business logic validation
    const customerAmount = parseFloat($('#CustomerInvoiceAmount').val()) || 0;
    const oemAmount = parseFloat($('#OemQuoteAmount').val()) || 0;
    
    if (customerAmount > 0 && oemAmount > 0 && customerAmount < oemAmount) {
        showProfessionalAlert('Warning', 'Customer invoice amount is less than OEM quote amount. Please verify pricing.', 'warning');
    }
    
    return isValid;
}

// ================================================
// DYNAMIC DROPDOWN SYSTEM
// ================================================

function initializeDynamicDropdowns() {
    // Professional cascade dropdown for Company -> Contacts
    $('#CompanyId').on('change', function() {
        const companyId = $(this).val();
        loadContactsByCompany(companyId);
    });
    
    // Professional cascade dropdown for OEM -> Products
    $('#OemId').on('change', function() {
        const oemId = $(this).val();
        loadProductsByOem(oemId);
    });
    
    // Product selection with auto-fill pricing
    $('#ProductId').on('change', function() {
        const productId = $(this).val();
        if (productId) {
            loadProductDetails(productId);
        }
    });
}

function loadContactsByCompany(companyId) {
    if (!companyId) {
        $('#ContactId').html('<option value="">Select Contact</option>').prop('disabled', true);
        return;
    }
    
    // Professional loading state
    $('#ContactId').html('<option value="">Loading contacts...</option>').prop('disabled', true);
    
    $.get(`/Deals/GetContactsByCompany?companyId=${companyId}`)
        .done(function(data) {
            let options = '<option value="">Select Contact</option>';
            data.forEach(contact => {
                options += `<option value="${contact.value}">${contact.text}</option>`;
            });
            $('#ContactId').html(options).prop('disabled', false);
            
            // Add professional animation
            $('#ContactId').closest('.form-group').addClass('updated');
            setTimeout(() => $('#ContactId').closest('.form-group').removeClass('updated'), 300);
        })
        .fail(function() {
            $('#ContactId').html('<option value="">Error loading contacts</option>');
            showProfessionalAlert('Error', 'Failed to load contacts. Please try again.', 'error');
        });
}

function loadProductsByOem(oemId) {
    if (!oemId) {
        $('#ProductId').html('<option value="">Select Product</option>').prop('disabled', true);
        return;
    }
    
    // Professional loading state
    $('#ProductId').html('<option value="">Loading products...</option>').prop('disabled', true);
    
    $.get(`/Deals/GetProductsByOem?oemId=${oemId}`)
        .done(function(data) {
            let options = '<option value="">Select Product</option>';
            data.forEach(product => {
                options += `<option value="${product.value}" 
                           data-price="${product.unitPrice}" 
                           data-category="${product.category}"
                           data-license-type="${product.licenseType}">
                           ${product.text}
                           </option>`;
            });
            $('#ProductId').html(options).prop('disabled', false);
            
            // Add professional animation
            $('#ProductId').closest('.form-group').addClass('updated');
            setTimeout(() => $('#ProductId').closest('.form-group').removeClass('updated'), 300);
        })
        .fail(function() {
            $('#ProductId').html('<option value="">Error loading products</option>');
            showProfessionalAlert('Error', 'Failed to load products. Please try again.', 'error');
        });
}

function loadProductDetails(productId) {
    const selectedOption = $('#ProductId option:selected');
    const unitPrice = selectedOption.data('price');
    const category = selectedOption.data('category');
    const licenseType = selectedOption.data('license-type');
    
    // Auto-fill product details in professional UI
    if (unitPrice) {
        $('#UnitPrice').val(unitPrice).trigger('input');
        
        // Show product information in professional card
        showProductInfoCard({
            category: category,
            licenseType: licenseType,
            unitPrice: unitPrice
        });
    }
}

// ================================================
// FINANCIAL CALCULATIONS
// ================================================

function calculateDealFinancials() {
    const quantity = parseFloat($('#Quantity').val()) || 0;
    const unitPrice = parseFloat($('#UnitPrice').val()) || 0;
    const customerAmount = parseFloat($('#CustomerInvoiceAmount').val()) || 0;
    const oemAmount = parseFloat($('#OemQuoteAmount').val()) || 0;
    
    // Calculate total amount
    const totalAmount = quantity * unitPrice;
    
    // Calculate estimated margin
    let estimatedMargin = 0;
    if (customerAmount > 0 && oemAmount > 0) {
        estimatedMargin = ((customerAmount - oemAmount) / customerAmount * 100);
    }
    
    // Update UI with professional formatting
    updateFinancialDisplay({
        totalAmount: totalAmount,
        estimatedMargin: estimatedMargin,
        customerAmount: customerAmount,
        oemAmount: oemAmount
    });
}

function updateFinancialDisplay(financials) {
    // Update estimated margin field
    $('#EstimatedMargin').val(financials.estimatedMargin.toFixed(2));
    
    // Update professional financial summary card
    updateFinancialSummaryCard(financials);
    
    // Add visual indicators for margin health
    const marginIndicator = $('#margin-indicator');
    marginIndicator.removeClass('good average poor');
    
    if (financials.estimatedMargin > 20) {
        marginIndicator.addClass('good').text('Excellent Margin');
    } else if (financials.estimatedMargin > 10) {
        marginIndicator.addClass('average').text('Good Margin');
    } else if (financials.estimatedMargin > 0) {
        marginIndicator.addClass('poor').text('Low Margin');
    } else {
        marginIndicator.addClass('poor').text('Negative Margin');
    }
}

// ================================================
// KPI DASHBOARD SYSTEM
// ================================================

function initializeKPIDashboard() {
    if ($('.dashboard-page').length > 0) {
        loadKPIStatistics();
        initializeCharts();
        
        // Auto-refresh KPIs every 5 minutes for professional real-time experience
        setInterval(loadKPIStatistics, 300000);
    }
}

function loadKPIStatistics() {
    $.get('/Deals/GetDealStatistics')
        .done(function(data) {
            updateKPICards(data);
            updatePhaseProgress(data);
        })
        .fail(function() {
            console.error('Failed to load KPI statistics');
        });
}

function updateKPICards(data) {
    // Animate KPI card updates with professional transitions
    $('.kpi-card[data-metric="total-deals"] .kpi-value').animateNumber(data.totalDeals);
    $('.kpi-card[data-metric="active-deals"] .kpi-value').animateNumber(data.activeDeals);
    $('.kpi-card[data-metric="won-deals"] .kpi-value').animateNumber(data.wonDeals);
    $('.kpi-card[data-metric="lost-deals"] .kpi-value').animateNumber(data.lostDeals);
    
    // Update phase indicators
    updatePhaseIndicators(data);
}

function updatePhaseIndicators(data) {
    const phaseData = [
        { phase: 'phase1', count: data.phase1, label: 'Leads' },
        { phase: 'phase2', count: data.phase2, label: 'Quoted' },
        { phase: 'phase3', count: data.phase3, label: 'Negotiation' },
        { phase: 'phase4', count: data.phase4, label: 'Won' }
    ];
    
    phaseData.forEach(phase => {
        $(`.phase-indicator[data-phase="${phase.phase}"] .phase-count`).animateNumber(phase.count);
        
        // Calculate and update progress percentage
        const percentage = data.totalDeals > 0 ? (phase.count / data.totalDeals * 100) : 0;
        $(`.phase-indicator[data-phase="${phase.phase}"] .progress-bar`).css('width', percentage + '%');
    });
}

// ================================================
// ADVANCED FILTERING SYSTEM
// ================================================

function initializeAdvancedFiltering() {
    // Professional search with debouncing
    let searchTimeout;
    $('#deal-search').on('input', function() {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            performAdvancedSearch();
        }, 500);
    });
    
    // Professional filter controls
    $('.filter-control').on('change', function() {
        performAdvancedSearch();
    });
    
    // Quick filter buttons
    $('.quick-filter-btn').on('click', function() {
        const filterType = $(this).data('filter');
        applyQuickFilter(filterType);
    });
}

function performAdvancedSearch() {
    const searchTerm = $('#deal-search').val();
    const stageFilter = $('#stage-filter').val();
    const assignedFilter = $('#assigned-filter').val();
    
    // Build professional search URL
    const params = new URLSearchParams();
    if (searchTerm) params.append('searchString', searchTerm);
    if (stageFilter) params.append('stage', stageFilter);
    if (assignedFilter) params.append('assignedTo', assignedFilter);
    
    // Update URL and reload content with professional loading
    const newUrl = '/Deals?' + params.toString();
    showTableLoading();
    window.location.href = newUrl;
}

// ================================================
// PROFESSIONAL UI ENHANCEMENTS
// ================================================

function initializeProfessionalAnimations() {
    // Smooth card animations
    $('.deal-card, .kpi-card').hover(
        function() { $(this).addClass('card-hover'); },
        function() { $(this).removeClass('card-hover'); }
    );
    
    // Professional button interactions
    $('.btn-professional').on('click', function() {
        $(this).addClass('btn-clicked');
        setTimeout(() => $(this).removeClass('btn-clicked'), 200);
    });
}

function showProfessionalAlert(title, message, type = 'info') {
    const alertClass = type === 'error' ? 'alert-danger' : 
                      type === 'warning' ? 'alert-warning' : 'alert-info';
    
    const alertHtml = `
        <div class="alert ${alertClass} alert-dismissible professional-alert" role="alert">
            <strong>${title}:</strong> ${message}
            <button type="button" class="close" data-dismiss="alert">
                <span>&times;</span>
            </button>
        </div>
    `;
    
    $('.alert-container').html(alertHtml);
    $('.professional-alert').slideDown(300);
    
    // Auto-dismiss after 5 seconds
    setTimeout(() => {
        $('.professional-alert').slideUp(300);
    }, 5000);
}

function showTableLoading() {
    $('.deals-table-container').addClass('loading');
    $('.table-loading-overlay').show();
}

function hideTableLoading() {
    $('.deals-table-container').removeClass('loading');
    $('.table-loading-overlay').hide();
}

// ================================================
// UTILITY FUNCTIONS
// ================================================

// Professional number animation for KPIs
$.fn.animateNumber = function(endValue) {
    return this.each(function() {
        const $this = $(this);
        const startValue = parseInt($this.text()) || 0;
        
        $({ value: startValue }).animate({ value: endValue }, {
            duration: 1000,
            easing: 'easeOutCubic',
            step: function() {
                $this.text(Math.floor(this.value));
            },
            complete: function() {
                $this.text(endValue);
            }
        });
    });
};

// Professional responsive table initialization
function initializeResponsiveTables() {
    if ($(window).width() < 992) {
        $('.deals-table').addClass('table-responsive-stack');
    }
    
    $(window).on('resize', function() {
        if ($(window).width() < 992) {
            $('.deals-table').addClass('table-responsive-stack');
        } else {
            $('.deals-table').removeClass('table-responsive-stack');
        }
    });
}

// Professional notification system
function initializeNotifications() {
    // Check for server-side messages
    if ($('.alert').length > 0) {
        $('.alert').each(function() {
            $(this).slideDown(300);
        });
    }
}

// Product information card display
function showProductInfoCard(product) {
    const cardHtml = `
        <div class="product-info-card">
            <h6>Product Details</h6>
            <div class="product-detail">
                <span class="label">Category:</span>
                <span class="value">${product.category || 'N/A'}</span>
            </div>
            <div class="product-detail">
                <span class="label">License Type:</span>
                <span class="value">${product.licenseType || 'N/A'}</span>
            </div>
            <div class="product-detail">
                <span class="label">Unit Price:</span>
                <span class="value">$${product.unitPrice || '0.00'}</span>
            </div>
        </div>
    `;
    
    $('#product-info-container').html(cardHtml).slideDown(300);
}

// Financial summary card update
function updateFinancialSummaryCard(financials) {
    const summaryHtml = `
        <div class="financial-summary-card">
            <h6>Financial Summary</h6>
            <div class="financial-row">
                <span class="label">Total Amount:</span>
                <span class="value">$${financials.totalAmount.toFixed(2)}</span>
            </div>
            <div class="financial-row">
                <span class="label">Customer Amount:</span>
                <span class="value">$${financials.customerAmount.toFixed(2)}</span>
            </div>
            <div class="financial-row">
                <span class="label">OEM Amount:</span>
                <span class="value">$${financials.oemAmount.toFixed(2)}</span>
            </div>
            <div class="financial-row highlight">
                <span class="label">Estimated Margin:</span>
                <span class="value" id="margin-indicator">${financials.estimatedMargin.toFixed(2)}%</span>
            </div>
        </div>
    `;
    
    $('#financial-summary-container').html(summaryHtml);
}

// Quick filter application
function applyQuickFilter(filterType) {
    $('.quick-filter-btn').removeClass('active');
    $(`.quick-filter-btn[data-filter="${filterType}"]`).addClass('active');
    
    switch (filterType) {
        case 'all':
            $('#stage-filter').val('').trigger('change');
            break;
        case 'active':
            $('#stage-filter').val('').trigger('change');
            // Add custom logic for active deals
            break;
        case 'won':
            $('#stage-filter').val('Won').trigger('change');
            break;
        case 'lost':
            $('#stage-filter').val('Lost').trigger('change');
            break;
    }
}

// Console logging for development
console.log('CBMS Deal Management - Professional JavaScript Loaded Successfully');
console.log('Enterprise Edition - Desktop-First Design with Bigin.com Style');
console.log('Ready for 4-Phase B2B2B Workflow Management');
