// Landing Page JavaScript - Chỉ xử lý UI, không xử lý dữ liệu

function scrollToSection(id) {
    const element = document.getElementById(id);
    if (element) {
        const offset = 100;
        const elementPosition = element.getBoundingClientRect().top;
        const offsetPosition = elementPosition + window.pageYOffset - offset;
        
        window.scrollTo({
            top: offsetPosition,
            behavior: 'smooth'
        });
    }
}

function toggleMobileMenu() {
    const menu = document.getElementById('mobileMenu');
    if (menu) {
        menu.classList.toggle('show');
    }
}

function setSearchMode(mode) {
    const repairTab = document.getElementById('repairTab');
    const warrantyTab = document.getElementById('warrantyTab');
    const searchInput = document.getElementById('searchInput');
    const searchModeInput = document.getElementById('searchModeInput');
    const heroTitle = document.getElementById('heroTitle');
    const heroSubtitle = document.getElementById('heroSubtitle');
    const errorMsg = document.getElementById('errorMessage');
    
    if (!repairTab || !warrantyTab || !searchInput || !searchModeInput) return;
    
    if (mode === 'repair') {
        repairTab.classList.add('active');
        warrantyTab.classList.remove('active');
        searchInput.placeholder = 'Mã phiếu / SĐT...';
        searchModeInput.value = 'repair';
        if (heroTitle) heroTitle.textContent = 'Tra cứu trạng thái';
        if (heroSubtitle) heroSubtitle.textContent = 'Sửa chữa thiết bị';
    } else {
        repairTab.classList.remove('active');
        warrantyTab.classList.add('active');
        searchInput.placeholder = 'Số Serial / IMEI...';
        searchModeInput.value = 'warranty';
        if (heroTitle) heroTitle.textContent = 'Kiểm tra thời hạn';
        if (heroSubtitle) heroSubtitle.textContent = 'Bảo hành chính hãng';
    }
    
    if (errorMsg) {
        errorMsg.classList.remove('show');
    }
}

function closeWarrantyResult() {
    const resultCard = document.querySelector('.warranty-result-card');
    if (resultCard) {
        resultCard.remove();
    }
}

// Active nav link on scroll
window.addEventListener('scroll', function() {
    const sections = ['home', 'about', 'services', 'policy', 'contact'];
    const navLinks = document.querySelectorAll('.nav-links a');
    const scrollPos = window.scrollY + 150;
    
    sections.forEach((section, index) => {
        const element = document.getElementById(section);
        if (element) {
            const top = element.offsetTop;
            const bottom = top + element.offsetHeight;
            
            if (scrollPos >= top && scrollPos < bottom) {
                navLinks.forEach(link => link.classList.remove('active'));
                if (navLinks[index]) {
                    navLinks[index].classList.add('active');
                }
            }
        }
    });
});

// Initialize page based on current search mode
document.addEventListener('DOMContentLoaded', function() {
    const searchModeInput = document.getElementById('searchModeInput');
    if (searchModeInput) {
        const mode = searchModeInput.value || 'repair';
        setSearchMode(mode);
    }
});

