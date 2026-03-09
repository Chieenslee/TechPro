// Advanced Search with Filters
function initAdvancedSearch() {
    // Date range picker
    const dateInputs = document.querySelectorAll('input[type="date"]');
    dateInputs.forEach(input => {
        input.addEventListener('change', function() {
            applyFilters();
        });
    });

    // Status filter
    const statusSelect = document.getElementById('statusFilter');
    if (statusSelect) {
        statusSelect.addEventListener('change', applyFilters);
    }

    // Search input with debounce
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        let searchTimeout;
        searchInput.addEventListener('input', function() {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(applyFilters, 500);
        });
    }
}

function applyFilters() {
    const form = document.getElementById('searchForm');
    if (form) {
        form.submit();
    }
}

function clearFilters() {
    document.getElementById('searchInput').value = '';
    document.getElementById('statusFilter').value = 'all';
    document.querySelectorAll('input[type="date"]').forEach(input => input.value = '');
    applyFilters();
}

document.addEventListener('DOMContentLoaded', initAdvancedSearch);

