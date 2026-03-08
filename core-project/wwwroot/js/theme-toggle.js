// Theme Toggle System
(function() {
    // Get stored theme or default to light for login page
    const isLoginPage = window.location.pathname.includes('login');
    const storedTheme = localStorage.getItem('theme-preference') || (isLoginPage ? 'light' : 'light');
    
    // Apply saved theme on load
    function applyTheme(theme) {
        const htmlElement = document.documentElement;
        
        if (theme === 'dark') {
            htmlElement.classList.remove('light-theme');
            htmlElement.classList.add('dark-theme');
            localStorage.setItem('theme-preference', 'dark');
        } else {
            htmlElement.classList.remove('dark-theme');
            htmlElement.classList.add('light-theme');
            localStorage.setItem('theme-preference', 'light');
        }
        
        updateToggleButton(theme);
    }
    
    // Update button text and icon
    function updateToggleButton(theme) {
        const btn = document.getElementById('themeToggle');
        if (btn) {
            if (theme === 'dark') {
                btn.innerHTML = '☀️ Light Mode';
            } else {
                btn.innerHTML = '🌙 Dark Mode';
            }
        }
    }
    
    // Toggle theme
    function toggleTheme() {
        const htmlElement = document.documentElement;
        const isDark = htmlElement.classList.contains('dark-theme');
        const newTheme = isDark ? 'light' : 'dark';
        applyTheme(newTheme);
    }
    
    // Create toggle button
    function createToggleButton() {
        const existingBtn = document.getElementById('themeToggle');
        if (existingBtn) return; // Button already exists
        
        const btn = document.createElement('button');
        btn.id = 'themeToggle';
        btn.className = 'theme-toggle';
        btn.setAttribute('type', 'button');
        btn.innerHTML = storedTheme === 'dark' ? '☀️ Light Mode' : '🌙 Dark Mode';
        btn.addEventListener('click', toggleTheme);
        
        document.body.appendChild(btn);
    }
    
    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            applyTheme(storedTheme);
            createToggleButton();
        });
    } else {
        applyTheme(storedTheme);
        createToggleButton();
    }
    
    // Expose functions globally
    window.toggleTheme = toggleTheme;
    window.applyTheme = applyTheme;
})();
