// Theme Management System
const ThemeManager = (() => {
  const THEME_STORAGE_KEY = 'tvbooking-theme-preference';
  const LIGHT_THEME = 'light';
  const DARK_THEME = 'dark';

  // Get the system preference
  const getSystemPreference = () => {
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? DARK_THEME : LIGHT_THEME;
  };

  // Get the stored preference or system preference
  const getEffectiveTheme = () => {
    const stored = localStorage.getItem(THEME_STORAGE_KEY);
    return stored || getSystemPreference();
  };

  // Update the DOM and icon
  const applyTheme = (theme) => {
    const html = document.documentElement;
    const icon = document.getElementById('theme-icon');
    
    // Apply the theme
    if (theme === DARK_THEME) {
      html.setAttribute('data-theme', 'dark');
      if (icon) icon.className = 'bi bi-moon-stars-fill';
    } else {
      html.removeAttribute('data-theme');
      if (icon) icon.className = 'bi bi-sun-fill';
    }
    
    // Trigger custom event for other components to react to theme changes
    window.dispatchEvent(new CustomEvent('themeChanged', { detail: { theme } }));
  };

  // Toggle between themes
  const toggleTheme = () => {
    const current = getEffectiveTheme();
    const next = current === DARK_THEME ? LIGHT_THEME : DARK_THEME;
    
    // Store the preference
    localStorage.setItem(THEME_STORAGE_KEY, next);
    
    // Apply the new theme
    applyTheme(next);
  };

  // Listen for system theme changes (only when no manual override is set)
  const watchSystemPreference = () => {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    
    // Handle changes in system preference when no override is set
    mediaQuery.addEventListener('change', (e) => {
      const stored = localStorage.getItem(THEME_STORAGE_KEY);
      if (!stored) {
        // No manual override, apply system preference
        applyTheme(e.matches ? DARK_THEME : LIGHT_THEME);
      }
    });
  };

  // Initialize on page load
  const init = () => {
    // Apply initial theme
    applyTheme(getEffectiveTheme());
    
    // Set up the toggle button
    const toggleBtn = document.getElementById('theme-toggle');
    if (toggleBtn) {
      toggleBtn.addEventListener('click', (e) => {
        e.preventDefault();
        toggleTheme();
      });
    }
    
    // Watch for system preference changes
    watchSystemPreference();
  };

  return {
    init,
    toggleTheme,
    getEffectiveTheme
  };
})();

// Flatpickr Theme Integration and Initialization
const FlatpickrThemeIntegration = (() => {
  const initializeFlatpickr = () => {
    if (typeof flatpickr === 'undefined') return;
    
    // Initialize all date inputs
    flatpickr('.flatpickr-input', {
      dateFormat: 'Y/m/d',
      allowInput: true
    });
    
    // Initialize date-time inputs (Start and End datetime pickers)
    flatpickr('[name="Booking.Start"], [name="Booking.End"]', {
      enableTime: true,
      dateFormat: 'Y/m/d H:i',
      time_24hr: true,
      minuteIncrement: 1,
      allowInput: true
    });
  };

  const updateFlatpickrTheme = (theme) => {
    // Flatpickr CSS is handled entirely by CSS variables in site.css
    // No additional JS manipulation needed
  };

  const init = () => {
    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
      document.addEventListener('DOMContentLoaded', initializeFlatpickr);
    } else {
      initializeFlatpickr();
    }
    
    // Listen for theme changes
    window.addEventListener('themeChanged', (e) => {
      updateFlatpickrTheme(e.detail.theme);
    });
  };

  return { init };
})();

// Chart.js Theme Integration (for Statistics page)
const ChartThemeIntegration = (() => {
  const updateChartDefaults = (theme) => {
    if (typeof Chart === 'undefined') return;
    
    const isDark = theme === 'dark';
    Chart.defaults.color = isDark ? '#a0a0a0' : '#666666';
    Chart.defaults.borderColor = isDark ? '#404040' : '#e0e0e0';
    
    // Update existing charts if they exist
    Chart.helpers.each(Chart.instances, function(instance) {
      instance.update();
    });
  };

  const init = () => {
    // Set initial chart colors
    updateChartDefaults(ThemeManager.getEffectiveTheme());
    
    // Listen for theme changes
    window.addEventListener('themeChanged', (e) => {
      updateChartDefaults(e.detail.theme);
    });
  };

  return { init };
})();

// Initialize on page load
document.addEventListener('DOMContentLoaded', () => {
  ThemeManager.init();
  FlatpickrThemeIntegration.init();
  ChartThemeIntegration.init();
});