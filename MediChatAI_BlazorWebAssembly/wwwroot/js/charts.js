// Chart.js Integration for Analytics Dashboard
// Theme-aware chart rendering with dark mode support

let charts = {
    userGrowth: null,
    activityBreakdown: null,
    roleDistribution: null,
    activityTimeline: null,
    activityDistribution: null,
    weeklyComparison: null
};

// Get theme colors from CSS variables
function getThemeColors() {
    const isDark = document.documentElement.classList.contains('dark');
    const primaryColor = getComputedStyle(document.documentElement).getPropertyValue('--color-primary').trim() || '#3b82f6';
    const accentColor = getComputedStyle(document.documentElement).getPropertyValue('--color-accent').trim() || '#8b5cf6';

    return {
        primary: primaryColor,
        accent: accentColor,
        text: isDark ? '#f9fafb' : '#1f2937',
        gridColor: isDark ? 'rgba(255, 255, 255, 0.1)' : 'rgba(0, 0, 0, 0.1)',
        backgroundColor: isDark ? '#1f2937' : '#ffffff',
        chartColors: [
            primaryColor,
            accentColor,
            '#10b981',
            '#f59e0b',
            '#ef4444',
            '#8b5cf6',
            '#06b6d4',
            '#ec4899',
            '#84cc16',
            '#f97316'
        ]
    };
}

// Default chart options with theme support
function getDefaultOptions(isDark) {
    const colors = getThemeColors();

    return {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                labels: {
                    color: colors.text,
                    font: {
                        family: "'Helvetica Neue', Helvetica, Arial, sans-serif",
                        size: 12
                    },
                    padding: 15,
                    usePointStyle: true,
                    pointStyle: 'circle'
                }
            },
            tooltip: {
                backgroundColor: isDark ? 'rgba(31, 41, 55, 0.95)' : 'rgba(255, 255, 255, 0.95)',
                titleColor: colors.text,
                bodyColor: colors.text,
                borderColor: colors.gridColor,
                borderWidth: 1,
                padding: 12,
                displayColors: true,
                boxPadding: 6,
                usePointStyle: true,
                callbacks: {
                    label: function(context) {
                        let label = context.dataset.label || '';
                        if (label) {
                            label += ': ';
                        }
                        if (context.parsed.y !== null) {
                            label += context.parsed.y;
                        } else if (context.parsed !== null) {
                            label += context.parsed;
                        }
                        return label;
                    }
                }
            }
        },
        scales: {
            x: {
                ticks: {
                    color: colors.text,
                    font: {
                        size: 11
                    }
                },
                grid: {
                    color: colors.gridColor,
                    drawBorder: false
                }
            },
            y: {
                ticks: {
                    color: colors.text,
                    font: {
                        size: 11
                    },
                    precision: 0
                },
                grid: {
                    color: colors.gridColor,
                    drawBorder: false
                }
            }
        },
        interaction: {
            intersect: false,
            mode: 'index'
        },
        animation: {
            duration: 750,
            easing: 'easeInOutQuart'
        }
    };
}

// Render User Growth Chart
window.renderUserGrowthChart = function(data) {
    const ctx = document.getElementById('userGrowthChart');
    if (!ctx) {
        console.error('User Growth Chart canvas not found');
        return;
    }

    const isDark = document.documentElement.classList.contains('dark');
    const colors = getThemeColors();

    // Destroy existing chart
    if (charts.userGrowth) {
        charts.userGrowth.destroy();
    }

    const chartData = {
        labels: data.labels || [],
        datasets: [{
            label: 'New Users',
            data: data.data || [],
            backgroundColor: `${colors.primary}20`,
            borderColor: colors.primary,
            borderWidth: 3,
            fill: true,
            tension: 0.4,
            pointRadius: 4,
            pointHoverRadius: 6,
            pointBackgroundColor: colors.primary,
            pointBorderColor: '#fff',
            pointBorderWidth: 2,
            pointHoverBackgroundColor: colors.primary,
            pointHoverBorderColor: '#fff',
            pointHoverBorderWidth: 3
        }]
    };

    const options = getDefaultOptions(isDark);
    options.plugins.legend.display = false;
    options.scales.y.beginAtZero = true;

    charts.userGrowth = new Chart(ctx, {
        type: 'line',
        data: chartData,
        options: options
    });
};

// Render Activity Breakdown Chart
window.renderActivityBreakdownChart = function(data) {
    const ctx = document.getElementById('activityBreakdownChart');
    if (!ctx) {
        console.error('Activity Breakdown Chart canvas not found');
        return;
    }

    const isDark = document.documentElement.classList.contains('dark');
    const colors = getThemeColors();

    // Destroy existing chart
    if (charts.activityBreakdown) {
        charts.activityBreakdown.destroy();
    }

    const chartData = {
        labels: data.labels || [],
        datasets: [{
            data: data.data || [],
            backgroundColor: colors.chartColors,
            borderColor: isDark ? '#1f2937' : '#ffffff',
            borderWidth: 3,
            hoverOffset: 10
        }]
    };

    const options = getDefaultOptions(isDark);
    delete options.scales; // Remove scales for donut chart
    options.cutout = '65%'; // Make it a donut chart
    options.plugins.legend.position = 'right';
    options.plugins.legend.labels.padding = 20;

    charts.activityBreakdown = new Chart(ctx, {
        type: 'doughnut',
        data: chartData,
        options: options
    });
};

// Render Role Distribution Chart
window.renderRoleDistributionChart = function(data) {
    const ctx = document.getElementById('roleDistributionChart');
    if (!ctx) {
        console.error('Role Distribution Chart canvas not found');
        return;
    }

    const isDark = document.documentElement.classList.contains('dark');
    const colors = getThemeColors();

    // Destroy existing chart
    if (charts.roleDistribution) {
        charts.roleDistribution.destroy();
    }

    const chartData = {
        labels: data.labels || [],
        datasets: [{
            label: 'Users',
            data: data.data || [],
            backgroundColor: [
                `${colors.chartColors[0]}CC`,
                `${colors.chartColors[2]}CC`,
                `${colors.chartColors[4]}CC`
            ],
            borderColor: isDark ? '#1f2937' : '#ffffff',
            borderWidth: 2,
            borderRadius: 8,
            hoverBackgroundColor: [
                colors.chartColors[0],
                colors.chartColors[2],
                colors.chartColors[4]
            ]
        }]
    };

    const options = getDefaultOptions(isDark);
    options.indexAxis = 'y'; // Horizontal bar chart
    options.scales.x.beginAtZero = true;
    options.plugins.legend.display = false;

    charts.roleDistribution = new Chart(ctx, {
        type: 'bar',
        data: chartData,
        options: options
    });
};

// Render Activity Timeline Chart (Area Chart)
window.renderActivityTimeline = function(data) {
    const ctx = document.getElementById('activityTimelineChart');
    if (!ctx) {
        console.error('Activity Timeline Chart canvas not found');
        return;
    }

    const isDark = document.documentElement.classList.contains('dark');
    const colors = getThemeColors();

    // Destroy existing chart
    if (charts.activityTimeline) {
        charts.activityTimeline.destroy();
    }

    const chartData = {
        labels: data.labels || [],
        datasets: [{
            label: 'Daily Activities',
            data: data.data || [],
            backgroundColor: `${colors.primary}30`,
            borderColor: colors.primary,
            borderWidth: 3,
            fill: true,
            tension: 0.4,
            pointRadius: 0,
            pointHoverRadius: 6,
            pointBackgroundColor: colors.primary,
            pointBorderColor: '#fff',
            pointBorderWidth: 2,
            pointHoverBackgroundColor: colors.primary,
            pointHoverBorderColor: '#fff',
            pointHoverBorderWidth: 3
        }]
    };

    const options = getDefaultOptions(isDark);
    options.plugins.legend.display = false;
    options.scales.y.beginAtZero = true;
    options.interaction.intersect = false;
    options.interaction.mode = 'index';

    charts.activityTimeline = new Chart(ctx, {
        type: 'line',
        data: chartData,
        options: options
    });
};

// Render Peak Hours Heatmap (24x7 Grid)
window.renderPeakHoursHeatmap = function(data) {
    const container = document.getElementById('peakHoursHeatmap');
    if (!container) {
        console.error('Peak Hours Heatmap container not found');
        return;
    }

    const isDark = document.documentElement.classList.contains('dark');
    const colors = getThemeColors();

    // Clear previous content
    container.innerHTML = '';

    if (!data || !data.hours || data.hours.length === 0) {
        container.innerHTML = '<div class="text-center py-8 text-gray-500 dark:text-gray-400">No data available</div>';
        return;
    }

    // Enhanced color mapping for heatmap levels
    const cellColors = [
        isDark ? 'rgba(31, 41, 55, 0.6)' : 'rgba(243, 244, 246, 0.9)',
        `rgba(var(--color-primary-rgb, 59 130 246), 0.25)`,
        `rgba(var(--color-primary-rgb, 59 130 246), 0.5)`,
        `rgba(var(--color-primary-rgb, 59 130 246), 0.75)`,
        colors.primary
    ];

    const cellSize = 24;
    const cellGap = 8;
    const labelWidth = 40;
    const labelHeight = 30;
    const hours = 24;
    const days = 7;

    const svgWidth = hours * (cellSize + cellGap) + labelWidth + 10;
    const svgHeight = days * (cellSize + cellGap) + labelHeight + 10;

    let svg = `<svg width="100%" height="${svgHeight}" viewBox="0 0 ${svgWidth} ${svgHeight}" preserveAspectRatio="xMinYMin meet">`;

    // Day labels
    const dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
    dayNames.forEach((day, index) => {
        const y = index * (cellSize + cellGap) + cellSize/2 + labelHeight + 5;
        svg += `<text x="5" y="${y}" class="text-xs font-medium" fill="${isDark ? '#9ca3af' : '#6b7280'}" font-size="10" dominant-baseline="middle">${day}</text>`;
    });

    // Hour labels (show every 3 hours)
    for (let hour = 0; hour < hours; hour += 3) {
        const x = hour * (cellSize + cellGap) + labelWidth + cellSize/2;
        const label = hour === 0 ? '12a' : hour < 12 ? `${hour}a` : hour === 12 ? '12p' : `${hour-12}p`;
        svg += `<text x="${x}" y="20" class="text-xs font-medium" fill="${isDark ? '#9ca3af' : '#6b7280'}" font-size="10" text-anchor="middle">${label}</text>`;
    }

    // Render heatmap cells
    data.hours.forEach((cell, index) => {
        const x = cell.hour * (cellSize + cellGap) + labelWidth;
        const y = cell.day * (cellSize + cellGap) + labelHeight;
        const color = cellColors[cell.level] || cellColors[0];
        const strokeColor = isDark ? '#374151' : '#e5e7eb';

        const tooltipText = `${cell.dayName} ${cell.hour}:00 - ${cell.count} ${cell.count === 1 ? 'activity' : 'activities'}`;

        svg += `<rect
            x="${x}"
            y="${y}"
            width="${cellSize}"
            height="${cellSize}"
            rx="3"
            fill="${color}"
            stroke="${strokeColor}"
            stroke-width="1"
            class="peak-cell"
            data-tooltip="${tooltipText}"
            style="cursor: pointer; transition: all 0.2s;"
        />`;
    });

    svg += '</svg>';

    // Add hover styles
    // const style = document.createElement('style');
    // style.textContent = `
    //     .peak-cell:hover {
    //         transform: scale(1.2);
    //         filter: drop-shadow(0 2px 4px rgba(0, 0, 0, 0.2));
    //         stroke: ${colors.primary} !important;
    //         stroke-width: 2 !important;
    //     }
    // `;
    // document.head.appendChild(style);

    container.innerHTML = svg;

    // Add tooltip functionality with anti-flicker system
    const cells = container.querySelectorAll('.peak-cell');
    const tooltip = createTooltip();
    let currentCell = null;
    let showTimeout = null;
    let hideTimeout = null;

    cells.forEach(cell => {
        // Throttled position update (60fps = 16ms)
        const throttledPositionUpdate = throttle((e) => {
            if (currentCell === cell && tooltip.style.opacity === '1') {
                updateTooltipPosition(tooltip, e);
            }
        }, 16);

        cell.addEventListener('mouseenter', function(e) {
            // Clear any pending hide timeout
            if (hideTimeout) {
                clearTimeout(hideTimeout);
                hideTimeout = null;
            }

            currentCell = this;
            const text = this.getAttribute('data-tooltip');

            // Delay to prevent flicker (150ms)
            showTimeout = setTimeout(() => {
                if (currentCell === this) {
                    showTooltip(tooltip, text, e, 0);
                }
            }, 150);
        });

        cell.addEventListener('mouseleave', function() {
            // Clear any pending show timeout
            if (showTimeout) {
                clearTimeout(showTimeout);
                showTimeout = null;
            }

            currentCell = null;
            // Delay hiding to prevent flicker when moving between cells (250ms)
            hideTimeout = setTimeout(() => {
                hideTooltip(tooltip);
            }, 250);
        });

        // Throttled mousemove for smooth tooltip positioning
        cell.addEventListener('mousemove', throttledPositionUpdate);
    });
};

// Render Activity Distribution Chart (Doughnut)
window.renderActivityDistribution = function(data) {
    const ctx = document.getElementById('activityDistributionChart');
    if (!ctx) {
        console.error('Activity Distribution Chart canvas not found');
        return;
    }

    const isDark = document.documentElement.classList.contains('dark');
    const colors = getThemeColors();

    // Destroy existing chart
    if (charts.activityDistribution) {
        charts.activityDistribution.destroy();
    }

    const chartData = {
        labels: data.labels || [],
        datasets: [{
            data: data.data || [],
            backgroundColor: colors.chartColors,
            borderColor: isDark ? '#1f2937' : '#ffffff',
            borderWidth: 3,
            hoverOffset: 15
        }]
    };

    const options = getDefaultOptions(isDark);
    delete options.scales;
    options.cutout = '70%';
    options.plugins.legend.position = 'bottom';
    options.plugins.legend.labels.padding = 15;
    options.plugins.tooltip.callbacks = {
        label: function(context) {
            const label = context.label || '';
            const value = context.parsed || 0;
            const percentage = data.percentages ? data.percentages[context.dataIndex] : 0;
            return `${label}: ${value} (${percentage}%)`;
        }
    };

    charts.activityDistribution = new Chart(ctx, {
        type: 'doughnut',
        data: chartData,
        options: options
    });
};

// Render Weekly Comparison Chart (Grouped Bar)
window.renderWeeklyComparison = function(data) {
    const ctx = document.getElementById('weeklyComparisonChart');
    if (!ctx) {
        console.error('Weekly Comparison Chart canvas not found');
        return;
    }

    const isDark = document.documentElement.classList.contains('dark');
    const colors = getThemeColors();

    // Destroy existing chart
    if (charts.weeklyComparison) {
        charts.weeklyComparison.destroy();
    }

    const chartData = {
        labels: data.labels || [],
        datasets: [
            {
                label: 'This Week',
                data: data.thisWeek || [],
                backgroundColor: `${colors.primary}CC`,
                borderColor: colors.primary,
                borderWidth: 2,
                borderRadius: 6,
                hoverBackgroundColor: colors.primary
            },
            {
                label: 'Last Week',
                data: data.lastWeek || [],
                backgroundColor: `${colors.accent}80`,
                borderColor: colors.accent,
                borderWidth: 2,
                borderRadius: 6,
                hoverBackgroundColor: colors.accent
            }
        ]
    };

    const options = getDefaultOptions(isDark);
    options.scales.y.beginAtZero = true;
    options.plugins.legend.display = true;
    options.plugins.legend.position = 'top';

    charts.weeklyComparison = new Chart(ctx, {
        type: 'bar',
        data: chartData,
        options: options
    });
};

// Update all charts when theme changes
function updateChartsTheme() {
    // Re-render all charts with new theme
    if (charts.userGrowth) {
        const data = charts.userGrowth.data;
        renderUserGrowthChart({ labels: data.labels, data: data.datasets[0].data });
    }
    if (charts.activityBreakdown) {
        const data = charts.activityBreakdown.data;
        renderActivityBreakdownChart({ labels: data.labels, data: data.datasets[0].data });
    }
    if (charts.roleDistribution) {
        const data = charts.roleDistribution.data;
        renderRoleDistributionChart({ labels: data.labels, data: data.datasets[0].data });
    }
    if (charts.activityTimeline) {
        const data = charts.activityTimeline.data;
        renderActivityTimeline({ labels: data.labels, data: data.datasets[0].data });
    }
    if (charts.activityDistribution) {
        const data = charts.activityDistribution.data;
        renderActivityDistribution({ labels: data.labels, data: data.datasets[0].data });
    }
    if (charts.weeklyComparison) {
        const data = charts.weeklyComparison.data;
        const thisWeek = data.datasets[0].data;
        const lastWeek = data.datasets[1].data;
        renderWeeklyComparison({ labels: data.labels, thisWeek: thisWeek, lastWeek: lastWeek });
    }
}

// Listen for theme changes
if (typeof MutationObserver !== 'undefined') {
    const observer = new MutationObserver(function(mutations) {
        mutations.forEach(function(mutation) {
            if (mutation.attributeName === 'class') {
                updateChartsTheme();
            }
        });
    });

    observer.observe(document.documentElement, {
        attributes: true,
        attributeFilter: ['class']
    });
}

// Export chart as image
window.exportChartAsImage = function(chartId, filename) {
    const canvas = document.getElementById(chartId);
    if (!canvas) {
        console.error('Chart canvas not found:', chartId);
        return;
    }

    try {
        const url = canvas.toDataURL('image/png');
        const link = document.createElement('a');
        link.download = filename || 'chart.png';
        link.href = url;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    } catch (error) {
        console.error('Error exporting chart:', error);
        alert('Failed to export chart. Please try again.');
    }
};

// Render Activity Heatmap (Modern Full-Width Contribution Graph)
window.renderActivityHeatmap = function(data) {
    const container = document.getElementById('activityHeatmap');
    if (!container) {
        console.error('Activity Heatmap container not found');
        return;
    }

    const isDark = document.documentElement.classList.contains('dark');
    const colors = getThemeColors();

    // Clear previous heatmap
    container.innerHTML = '';

    if (!data || !data.days || data.days.length === 0) {
        container.innerHTML = '<div class="text-center py-12"><div class="inline-flex items-center px-6 py-3 bg-gray-100 dark:bg-gray-800 rounded-xl"><svg class="w-5 h-5 mr-2 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4"/></svg><span class="text-gray-500 dark:text-gray-400">No activity data available</span></div></div>';
        return;
    }

    // Enhanced color mapping with theme-aware gradients
    const cellColors = [
        isDark ? 'rgba(31, 41, 55, 0.8)' : 'rgba(235, 237, 240, 0.9)',  // Level 0 - no activity
        `rgba(var(--color-primary-rgb, 59 130 246), 0.2)`,               // Level 1 - low
        `rgba(var(--color-primary-rgb, 59 130 246), 0.45)`,              // Level 2 - medium-low
        `rgba(var(--color-primary-rgb, 59 130 246), 0.7)`,               // Level 3 - medium
        colors.primary                                                    // Level 4 - high
    ];

    // Enhanced styling with larger cells and better spacing
    const cellSize = 16;
    const cellGap = 8;
    const weeks = Math.ceil(data.days.length / 7);
    const daysPerWeek = 7;
    const labelWidth = 60;
    const labelHeight = 35;

    // Calculate responsive width
    const containerWidth = container.offsetWidth;
    const availableWidth = containerWidth - labelWidth + 80;
    const calculatedCellSize = Math.min(cellSize, Math.floor(availableWidth / weeks) - cellGap);
    const finalCellSize = Math.max(12, calculatedCellSize); // Minimum 12px

    // Create SVG with full width
    const svgWidth = weeks * (finalCellSize + cellGap) + labelWidth + 80;
    const svgHeight = daysPerWeek * (finalCellSize + cellGap) + labelHeight;

    let svg = `<svg width="100%" height="${svgHeight}" viewBox="0 0 ${svgWidth} ${svgHeight}" preserveAspectRatio="xMinYMin meet" class="heatmap-svg">`;

    // Add gradient definitions for glow effects
    svg += `<defs>
        <filter id="glow">
            <feGaussianBlur stdDeviation="2" result="coloredBlur"/>
            <feMerge>
                <feMergeNode in="coloredBlur"/>
                <feMergeNode in="SourceGraphic"/>
            </feMerge>
        </filter>
        <linearGradient id="headerGradient" x1="0%" y1="0%" x2="100%" y2="0%">
            <stop offset="0%" style="stop-color:${colors.primary};stop-opacity:0.1" />
            <stop offset="100%" style="stop-color:${colors.accent};stop-opacity:0.1" />
        </linearGradient>
    </defs>`;

    // Enhanced day labels with better styling
    const dayLabels = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
    const displayDays = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
    displayDays.forEach((label, index) => {
        if (label) {
            const y = index * (finalCellSize + cellGap) + finalCellSize + labelHeight - 8;
            svg += `<text x="0" y="${y}" class="text-xs font-medium" fill="${isDark ? '#9ca3af' : '#6b7280'}" font-size="11">${label}</text>`;
        }
    });

    // Enhanced month labels with background
    const months = [];
    let currentMonth = '';
    data.days.forEach((day, index) => {
        const date = new Date(day.date);
        const monthName = date.toLocaleDateString('en-US', { month: 'short' });
        if (monthName !== currentMonth) {
            currentMonth = monthName;
            const weekIndex = Math.floor(index / 7);
            const x = weekIndex * (finalCellSize + cellGap) + labelWidth;
            months.push({ name: monthName, x: x });
        }
    });

    months.forEach((month, idx) => {
        const nextX = months[idx + 1]?.x || svgWidth;
        const width = nextX - month.x;
        svg += `<rect x="${month.x}" y="0" width="${width}" height="25" fill="url(#headerGradient)" rx="6" opacity="0.3"/>`;
        svg += `<text x="${month.x + width/2}" y="16" class="text-xs font-semibold" text-anchor="middle" fill="${isDark ? '#d1d5db' : '#4b5563'}" font-size="12">${month.name}</text>`;
    });

    // Render enhanced cells with animations
    let animationDelay = 0;
    data.days.forEach((day, index) => {
        const weekIndex = Math.floor(index / 7);
        const dayIndex = day.dayOfWeek;
        const x = weekIndex * (finalCellSize + cellGap) + labelWidth;
        const y = dayIndex * (finalCellSize + cellGap) + labelHeight;

        const color = cellColors[day.level] || cellColors[0];
        const opacity = day.level === 0 ? 0.6 : 1;
        const strokeColor = isDark ? '#374151' : '#e5e7eb';
        const strokeWidth = day.level > 0 ? 1.5 : 1;

        // Create tooltip content
        const date = new Date(day.date);
        const formattedDate = date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
        let tooltipText = `${formattedDate}: ${day.count} ${day.count === 1 ? 'activity' : 'activities'}`;

        if (day.topActivities && day.topActivities.length > 0) {
            tooltipText += '\\n' + day.topActivities.map(a => `${a.type}: ${a.count}`).join(', ');
        }

        // Add subtle glow effect for high activity
        const glowFilter = day.level >= 3 ? 'filter="url(#glow)"' : '';

        svg += `<rect
            x="${x}"
            y="${y}"
            width="${finalCellSize}"
            height="${finalCellSize}"
            rx="3"
            fill="${color}"
            opacity="${opacity}"
            stroke="${strokeColor}"
            stroke-width="${strokeWidth}"
            class="heatmap-cell"
            data-date="${day.date}"
            data-count="${day.count}"
            data-level="${day.level}"
            data-tooltip="${tooltipText}"
            ${glowFilter}
            style="cursor: pointer; transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1); animation: fadeIn 0.5s ease-out ${animationDelay}s both;"
        />`;

        animationDelay += 0.005; // Stagger animation
    });

    svg += '</svg>';

    // Add CSS animation
    // const style = document.createElement('style');
    // style.textContent = `
    //     @keyframes fadeIn {
    //         from {
    //             opacity: 0;
    //             transform: scale(0.8);
    //         }
    //         to {
    //             opacity: 1;
    //             transform: scale(1);
    //         }
    //     }
    //     .heatmap-cell:hover {
    //         transform: scale(1.3) !important;
    //         opacity: 1 !important;
    //         filter: drop-shadow(0 4px 6px rgba(0, 0, 0, 0.2)) !important;
    //         stroke-width: 2.5 !important;
    //         stroke: ${colors.primary} !important;
    //     }
    //     .heatmap-svg {
    //         overflow: visible;
    //     }
    // `;
    // document.head.appendChild(style);

    container.innerHTML = svg;

    // Add enhanced hover effects and tooltip with improved anti-flicker
    const cells = container.querySelectorAll('.heatmap-cell');
    const tooltip = createTooltip();
    let currentCell = null;
    let showTimeout = null;
    let hideTimeout = null;

    cells.forEach(cell => {
        // Throttled position update (60fps = 16ms)
        const throttledPositionUpdate = throttle((e) => {
            if (currentCell === cell && tooltip.style.opacity === '1') {
                updateTooltipPosition(tooltip, e);
            }
        }, 16);

        cell.addEventListener('mouseenter', function(e) {
            // Clear any pending hide timeout
            if (hideTimeout) {
                clearTimeout(hideTimeout);
                hideTimeout = null;
            }

            currentCell = this;
            const tooltipText = this.getAttribute('data-tooltip');
            const level = this.getAttribute('data-level');

            // Increased delay to prevent flicker (150ms)
            showTimeout = setTimeout(() => {
                if (currentCell === this) {
                    showTooltip(tooltip, tooltipText, e, level);
                }
            }, 150);
        });

        cell.addEventListener('mouseleave', function() {
            // Clear any pending show timeout
            if (showTimeout) {
                clearTimeout(showTimeout);
                showTimeout = null;
            }

            currentCell = null;
            // Increased delay to prevent flicker when moving between cells (250ms)
            hideTimeout = setTimeout(() => {
                hideTooltip(tooltip);
            }, 250);
        });

        // Throttled mousemove for smooth, performant tooltip positioning
        cell.addEventListener('mousemove', throttledPositionUpdate);
    });

    // Hide tooltip when scrolling
    container.addEventListener('scroll', () => {
        hideTooltip(tooltip);
    });
};

// Throttle function to limit how often a function can be called
function throttle(func, limit) {
    let inThrottle;
    return function(...args) {
        if (!inThrottle) {
            func.apply(this, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    };
}

// Enhanced Tooltip helper functions
function createTooltip() {
    let tooltip = document.getElementById('heatmap-tooltip');
    if (!tooltip) {
        tooltip = document.createElement('div');
        tooltip.id = 'heatmap-tooltip';
        // Removed opacity-0 class to prevent conflicts with inline styles
        tooltip.className = 'fixed px-4 py-3 text-sm text-white rounded-xl pointer-events-none whitespace-pre-line backdrop-blur-xl border';

        // Set initial styles via JavaScript for full control
        tooltip.style.zIndex = '9999';
        tooltip.style.opacity = '0';  // Set via inline style instead of class
        tooltip.style.display = 'block';  // Ensure it's block-level for proper dimensions
        tooltip.style.visibility = 'hidden';  // Hide initially but maintain layout
        tooltip.style.transition = 'opacity 0.2s cubic-bezier(0.4, 0, 0.2, 1), transform 0.2s cubic-bezier(0.4, 0, 0.2, 1)';
        tooltip.style.willChange = 'opacity, transform';
        tooltip.style.textShadow = '0 1px 2px rgba(0, 0, 0, 0.3)';
        tooltip.style.maxWidth = '300px';
        tooltip.style.lineHeight = '1.5';

        // Set initial position off-screen to prevent flash of unstyled content
        tooltip.style.left = '-9999px';
        tooltip.style.top = '-9999px';

        document.body.appendChild(tooltip);
    }
    return tooltip;
}

function showTooltip(tooltip, text, event, level) {
    const colors = getThemeColors();

    // Consistently dark background for maximum readability and contrast
    // Using 98% opacity dark background with white text ensures WCAG AAA compliance
    tooltip.style.background = 'linear-gradient(135deg, rgba(17, 24, 39, 0.98), rgba(31, 41, 55, 0.98))';
    tooltip.style.color = '#ffffff';
    tooltip.style.fontWeight = '600';

    // Add subtle colored border for high-activity cells
    if (level >= 3) {
        tooltip.style.borderColor = colors.primary;
        tooltip.style.borderWidth = '2px';
        tooltip.style.boxShadow = `0 10px 25px rgba(0, 0, 0, 0.5), 0 0 0 1px ${colors.primary}40`;
    } else {
        tooltip.style.borderColor = 'rgba(255, 255, 255, 0.2)';
        tooltip.style.borderWidth = '1px';
        tooltip.style.boxShadow = '0 10px 25px rgba(0, 0, 0, 0.5)';
    }

    tooltip.textContent = text;

    // Make visible BEFORE positioning to get accurate dimensions
    tooltip.style.visibility = 'visible';
    tooltip.style.opacity = '1';
    tooltip.style.transform = 'scale(1)';

    // Position after making visible so getBoundingClientRect() returns correct dimensions
    updateTooltipPosition(tooltip, event);
}

function hideTooltip(tooltip) {
    tooltip.style.opacity = '0';
    tooltip.style.transform = 'scale(0.95)';
    tooltip.style.visibility = 'hidden';

    // Reset position off-screen after animation completes
    setTimeout(() => {
        if (tooltip.style.opacity === '0') {
            tooltip.style.left = '-9999px';
            tooltip.style.top = '-9999px';
        }
    }, 300);  // Wait for 200ms transition + buffer
}

function updateTooltipPosition(tooltip, event) {
    // Use fixed offset for stable positioning
    const offset = 25;

    // Get tooltip dimensions
    const tooltipRect = tooltip.getBoundingClientRect();
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;

    // Default position: bottom-right of cursor
    let x = event.clientX + offset;
    let y = event.clientY + offset;

    // Smart positioning: flip to left if would go off-screen
    if (x + tooltipRect.width > viewportWidth - 10) {
        x = event.clientX - tooltipRect.width - offset;
    }

    // Smart positioning: flip to top if would go off-screen
    if (y + tooltipRect.height > viewportHeight - 10) {
        y = event.clientY - tooltipRect.height - offset;
    }

    // Ensure tooltip stays within viewport bounds
    x = Math.max(10, Math.min(x, viewportWidth - tooltipRect.width - 10));
    y = Math.max(10, Math.min(y, viewportHeight - tooltipRect.height - 10));

    // Use transform for better performance (GPU-accelerated)
    tooltip.style.left = `${x}px`;
    tooltip.style.top = `${y}px`;
}

// Report Export Functionality

// Download file helper (named differently to avoid conflict with attachments.js/fileDownload.js)
window.downloadFileContent = function(filename, content, mimeType) {
    console.log('downloadFileContent called:', filename, 'content length:', content?.length, 'mimeType:', mimeType);
    
    // For CSV files, add BOM for proper UTF-8 encoding in Excel
    let fileContent = content;
    if (mimeType && mimeType.includes('csv')) {
        fileContent = '\uFEFF' + content; // Add UTF-8 BOM
    }
    
    const blob = new Blob([fileContent], { type: mimeType || 'text/plain;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.style.display = 'none';
    document.body.appendChild(link);
    
    // Use setTimeout to ensure the click happens
    setTimeout(() => {
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
        console.log('File download initiated:', filename);
    }, 100);
};

// Export report as PDF (requires html2canvas and jsPDF libraries)
window.exportReportAsPDF = async function(elementId, filename) {
    try {
        const element = document.getElementById(elementId);
        if (!element) {
            console.error('Element not found:', elementId);
            return false;
        }

        // Check if html2canvas is available
        if (typeof html2canvas === 'undefined') {
            console.warn('html2canvas library not loaded. PDF export not available.');
            alert('PDF export requires additional libraries. Please use PNG export instead.');
            return false;
        }

        const canvas = await html2canvas(element, {
            scale: 2,
            logging: false,
            backgroundColor: document.documentElement.classList.contains('dark') ? '#1f2937' : '#ffffff'
        });

        // Check if jsPDF is available
        if (typeof jspdf === 'undefined' && typeof window.jspdf === 'undefined') {
            // Fallback to PNG if jsPDF not available
            const imgData = canvas.toDataURL('image/png');
            const link = document.createElement('a');
            link.download = filename.replace('.pdf', '.png');
            link.href = imgData;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            console.warn('jsPDF library not loaded. Exported as PNG instead.');
            return true;
        }

        const pdf = new jspdf.jsPDF({
            orientation: canvas.width > canvas.height ? 'landscape' : 'portrait',
            unit: 'mm',
            format: 'a4'
        });

        const imgData = canvas.toDataURL('image/png');
        const pdfWidth = pdf.internal.pageSize.getWidth();
        const pdfHeight = (canvas.height * pdfWidth) / canvas.width;

        pdf.addImage(imgData, 'PNG', 0, 0, pdfWidth, pdfHeight);
        pdf.save(filename || 'report.pdf');
        return true;
    } catch (error) {
        console.error('Error exporting PDF:', error);
        return false;
    }
};

// Export data as CSV
window.exportDataAsCSV = function(data, filename, headers) {
    try {
        let csv = '';

        // Add headers if provided
        if (headers && headers.length > 0) {
            csv += headers.join(',') + '\n';
        }

        // Add data rows
        if (Array.isArray(data)) {
            data.forEach(row => {
                if (Array.isArray(row)) {
                    csv += row.map(cell => {
                        // Escape commas and quotes
                        const cellStr = String(cell);
                        if (cellStr.includes(',') || cellStr.includes('"') || cellStr.includes('\n')) {
                            return '"' + cellStr.replace(/"/g, '""') + '"';
                        }
                        return cellStr;
                    }).join(',') + '\n';
                } else if (typeof row === 'object') {
                    // Convert object to array of values
                    const values = Object.values(row).map(cell => {
                        const cellStr = String(cell);
                        if (cellStr.includes(',') || cellStr.includes('"') || cellStr.includes('\n')) {
                            return '"' + cellStr.replace(/"/g, '""') + '"';
                        }
                        return cellStr;
                    });
                    csv += values.join(',') + '\n';
                }
            });
        }

        downloadFileContent(filename || 'report.csv', csv, 'text/csv;charset=utf-8;');
        return true;
    } catch (error) {
        console.error('Error exporting CSV:', error);
        return false;
    }
};

// Export data as Excel (requires SheetJS library)
window.exportDataAsExcel = function(data, filename, sheetName) {
    try {
        // Check if SheetJS is available
        if (typeof XLSX === 'undefined') {
            console.warn('SheetJS library not loaded. Falling back to CSV export.');
            return exportDataAsCSV(data, filename.replace('.xlsx', '.csv'));
        }

        // Create workbook
        const wb = XLSX.utils.book_new();

        // Convert data to worksheet
        let ws;
        if (Array.isArray(data) && data.length > 0) {
            ws = XLSX.utils.json_to_sheet(data);
        } else {
            ws = XLSX.utils.aoa_to_sheet([['No data available']]);
        }

        // Add worksheet to workbook
        XLSX.utils.book_append_sheet(wb, ws, sheetName || 'Report');

        // Generate Excel file and trigger download
        XLSX.writeFile(wb, filename || 'report.xlsx');
        return true;
    } catch (error) {
        console.error('Error exporting Excel:', error);
        return false;
    }
};

// Export data as JSON
window.exportDataAsJSON = function(data, filename) {
    try {
        const jsonString = JSON.stringify(data, null, 2);
        downloadFileContent(filename || 'report.json', jsonString, 'application/json;charset=utf-8;');
        return true;
    } catch (error) {
        console.error('Error exporting JSON:', error);
        return false;
    }
};

// Export chart as high-quality PNG
window.exportChartAsPNG = function(chartId, filename) {
    const canvas = document.getElementById(chartId);
    if (!canvas) {
        console.error('Chart canvas not found:', chartId);
        return false;
    }

    try {
        // Export at higher resolution for better quality
        const url = canvas.toDataURL('image/png', 1.0);
        const link = document.createElement('a');
        link.download = filename || 'chart.png';
        link.href = url;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        return true;
    } catch (error) {
        console.error('Error exporting chart as PNG:', error);
        return false;
    }
};

// Export entire report section as image
window.exportReportSectionAsImage = async function(sectionId, filename) {
    try {
        const section = document.getElementById(sectionId);
        if (!section) {
            console.error('Section not found:', sectionId);
            return false;
        }

        // Check if html2canvas is available
        if (typeof html2canvas === 'undefined') {
            console.warn('html2canvas library not loaded. Image export not available.');
            alert('Image export requires html2canvas library.');
            return false;
        }

        const canvas = await html2canvas(section, {
            scale: 2,
            logging: false,
            backgroundColor: document.documentElement.classList.contains('dark') ? '#1f2937' : '#ffffff'
        });

        const url = canvas.toDataURL('image/png', 1.0);
        const link = document.createElement('a');
        link.download = filename || 'report-section.png';
        link.href = url;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        return true;
    } catch (error) {
        console.error('Error exporting section as image:', error);
        return false;
    }
};

// Print report
window.printReport = function(elementId) {
    try {
        const element = document.getElementById(elementId);
        if (!element) {
            console.error('Element not found:', elementId);
            return false;
        }

        const printWindow = window.open('', '_blank');
        printWindow.document.write(`
            <!DOCTYPE html>
            <html>
            <head>
                <title>Print Report</title>
                <style>
                    body {
                        font-family: Arial, sans-serif;
                        margin: 20px;
                    }
                    @media print {
                        .no-print { display: none; }
                    }
                </style>
            </head>
            <body>
                ${element.innerHTML}
            </body>
            </html>
        `);
        printWindow.document.close();
        printWindow.focus();
        setTimeout(() => {
            printWindow.print();
            printWindow.close();
        }, 250);
        return true;
    } catch (error) {
        console.error('Error printing report:', error);
        return false;
    }
};

// Render Report Chart with Theme Support
window.renderReportChart = function(canvasId, config) {
    try {
        const ctx = document.getElementById(canvasId);
        if (!ctx) {
            console.error('Report chart canvas not found:', canvasId);
            return;
        }

        const isDark = document.documentElement.classList.contains('dark');

        // Use provided theme colors or fallback to defaults
        const primaryColor = config.primaryColor || '#3b82f6';
        const accentColor = config.accentColor || '#8b5cf6';
        const textColor = config.textColor || (isDark ? '#f9fafb' : '#1f2937');
        const gridColor = isDark ? 'rgba(255, 255, 255, 0.1)' : 'rgba(0, 0, 0, 0.1)';

        // Destroy existing chart if it exists
        const existingChart = Chart.getChart(canvasId);
        if (existingChart) {
            existingChart.destroy();
        }

        const chartType = config.type || 'bar';
        const labels = config.labels || [];
        const values = config.values || [];

        let chartData;
        let chartOptions;

        // Configure based on chart type
        switch (chartType) {
            case 'line':
                chartData = {
                    labels: labels,
                    datasets: [{
                        label: config.label || 'Data',
                        data: values,
                        backgroundColor: `${primaryColor}20`,
                        borderColor: primaryColor,
                        borderWidth: 3,
                        fill: true,
                        tension: 0.4,
                        pointRadius: 4,
                        pointHoverRadius: 6,
                        pointBackgroundColor: primaryColor,
                        pointBorderColor: '#fff',
                        pointBorderWidth: 2
                    }]
                };
                break;

            case 'area':
                chartData = {
                    labels: labels,
                    datasets: [{
                        label: config.label || 'Data',
                        data: values,
                        backgroundColor: `${primaryColor}40`,
                        borderColor: primaryColor,
                        borderWidth: 3,
                        fill: true,
                        tension: 0.3,
                        pointRadius: 0,
                        pointHoverRadius: 6
                    }]
                };
                break;

            case 'bar':
                chartData = {
                    labels: labels,
                    datasets: [{
                        label: config.label || 'Data',
                        data: values,
                        backgroundColor: `${primaryColor}CC`,
                        borderColor: primaryColor,
                        borderWidth: 2,
                        borderRadius: 8
                    }]
                };
                break;

            case 'pie':
            case 'donut':
                const colors = [primaryColor, accentColor, '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#06b6d4', '#ec4899'];
                chartData = {
                    labels: labels,
                    datasets: [{
                        data: values,
                        backgroundColor: colors,
                        borderColor: isDark ? '#1f2937' : '#ffffff',
                        borderWidth: 3,
                        hoverOffset: 10
                    }]
                };
                break;

            case 'radar':
                chartData = {
                    labels: labels,
                    datasets: [{
                        label: config.label || 'Metrics',
                        data: values,
                        backgroundColor: `${primaryColor}40`,
                        borderColor: primaryColor,
                        borderWidth: 2,
                        pointBackgroundColor: primaryColor,
                        pointBorderColor: '#fff',
                        pointHoverBackgroundColor: '#fff',
                        pointHoverBorderColor: primaryColor
                    }]
                };
                break;

            default:
                chartData = {
                    labels: labels,
                    datasets: [{
                        label: config.label || 'Data',
                        data: values,
                        backgroundColor: primaryColor
                    }]
                };
        }

        // Base options
        chartOptions = {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: chartType === 'pie' || chartType === 'donut' || chartType === 'radar',
                    position: chartType === 'pie' || chartType === 'donut' ? 'right' : 'top',
                    labels: {
                        color: textColor,
                        font: { size: 12 },
                        padding: 15,
                        usePointStyle: true
                    }
                },
                tooltip: {
                    backgroundColor: isDark ? 'rgba(31, 41, 55, 0.95)' : 'rgba(255, 255, 255, 0.95)',
                    titleColor: textColor,
                    bodyColor: textColor,
                    borderColor: gridColor,
                    borderWidth: 1,
                    padding: 12,
                    displayColors: true
                }
            },
            animation: {
                duration: 750,
                easing: 'easeInOutQuart'
            }
        };

        // Add scales for charts that need them
        if (chartType !== 'pie' && chartType !== 'donut' && chartType !== 'radar') {
            chartOptions.scales = {
                x: {
                    ticks: { color: textColor, font: { size: 11 } },
                    grid: { color: gridColor, drawBorder: false }
                },
                y: {
                    beginAtZero: true,
                    ticks: { color: textColor, font: { size: 11 }, precision: 0 },
                    grid: { color: gridColor, drawBorder: false }
                }
            };
        }

        // Special config for donut
        if (chartType === 'donut') {
            chartOptions.cutout = '65%';
        }

        // Create the chart
        new Chart(ctx, {
            type: chartType === 'donut' ? 'doughnut' : chartType === 'area' ? 'line' : chartType,
            data: chartData,
            options: chartOptions
        });

    } catch (error) {
        console.error('Error rendering report chart:', error);
    }
};

// Destroy all charts on page unload
window.addEventListener('beforeunload', function() {
    Object.values(charts).forEach(chart => {
        if (chart) chart.destroy();
    });

    // Remove tooltip
    const tooltip = document.getElementById('heatmap-tooltip');
    if (tooltip) tooltip.remove();
});

// Professional PDF Report Generator
window.generateProfessionalReport = async function(reportConfig) {
    try {
        console.log('Starting professional report generation...', reportConfig);

        // Populate report header
        document.getElementById('reportTitle').textContent = reportConfig.title || 'Report';
        document.getElementById('reportSubtitle').textContent = reportConfig.subtitle || 'MediChat.AI Analytics Report';
        document.getElementById('reportDate').textContent = `Generated on: ${new Date().toLocaleDateString('en-US', { month: 'long', day: 'numeric', year: 'numeric', hour: '2-digit', minute: '2-digit' })}`;

        // Populate metrics summary
        const metricsContainer = document.getElementById('reportMetrics');
        metricsContainer.innerHTML = '';

        if (reportConfig.metrics && reportConfig.metrics.length > 0) {
            reportConfig.metrics.forEach(metric => {
                const metricCard = document.createElement('div');
                metricCard.className = 'bg-white border-2 border-gray-200 rounded-lg p-4 text-center';
                metricCard.innerHTML = `
                    <div class="text-3xl font-bold text-blue-600">${metric.value}</div>
                    <div class="text-sm text-gray-600 mt-1">${metric.label}</div>
                `;
                metricsContainer.appendChild(metricCard);
            });
        } else {
            metricsContainer.innerHTML = '<div class="col-span-4 text-center text-gray-500">No summary metrics available</div>';
        }

        // Populate data table if provided
        const tableSection = document.getElementById('reportTableSection');
        const tableContent = document.getElementById('reportTableContent');

        if (reportConfig.tableData && reportConfig.tableData.length > 0) {
            tableSection.style.display = 'block';
            let tableHTML = '<table class="min-w-full border-collapse border border-gray-300" style="width: 100%;">';

            // Table headers
            if (reportConfig.tableHeaders && reportConfig.tableHeaders.length > 0) {
                tableHTML += '<thead><tr class="bg-gray-100">';
                reportConfig.tableHeaders.forEach(header => {
                    tableHTML += `<th class="border border-gray-300 px-4 py-2 text-left text-sm font-semibold text-gray-700">${header}</th>`;
                });
                tableHTML += '</tr></thead>';
            }

            // Table body
            tableHTML += '<tbody>';
            reportConfig.tableData.slice(0, 20).forEach((row, index) => {
                tableHTML += `<tr class="${index % 2 === 0 ? 'bg-white' : 'bg-gray-50'}">`;
                Object.values(row).forEach(cell => {
                    tableHTML += `<td class="border border-gray-300 px-4 py-2 text-sm text-gray-700">${cell}</td>`;
                });
                tableHTML += '</tr>';
            });
            tableHTML += '</tbody></table>';

            if (reportConfig.tableData.length > 20) {
                tableHTML += `<p class="text-sm text-gray-500 mt-2 text-center">Showing 20 of ${reportConfig.tableData.length} rows</p>`;
            }

            tableContent.innerHTML = tableHTML;
        } else {
            tableSection.style.display = 'none';
        }

        // Render chart
        const canvas = document.getElementById('reportChartCanvas');
        if (!canvas) {
            console.error('Report chart canvas not found');
            throw new Error('Chart canvas not found');
        }

        // Destroy existing chart if any
        const existingChart = Chart.getChart('reportChartCanvas');
        if (existingChart) {
            existingChart.destroy();
        }

        // Prepare chart data
        const chartType = reportConfig.chartType || 'bar';
        const labels = reportConfig.labels || [];
        const values = reportConfig.values || [];
        const primaryColor = reportConfig.primaryColor || '#3b82f6';
        const accentColor = reportConfig.accentColor || '#8b5cf6';

        let chartData, chartOptions;

        // Configure chart based on type
        switch (chartType) {
            case 'line':
                chartData = {
                    labels: labels,
                    datasets: [{
                        label: reportConfig.datasetLabel || 'Data',
                        data: values,
                        backgroundColor: `${primaryColor}30`,
                        borderColor: primaryColor,
                        borderWidth: 3,
                        fill: true,
                        tension: 0.4,
                        pointRadius: 4,
                        pointBackgroundColor: primaryColor,
                        pointBorderColor: '#fff',
                        pointBorderWidth: 2
                    }]
                };
                break;

            case 'bar':
                chartData = {
                    labels: labels,
                    datasets: [{
                        label: reportConfig.datasetLabel || 'Data',
                        data: values,
                        backgroundColor: `${primaryColor}CC`,
                        borderColor: primaryColor,
                        borderWidth: 2,
                        borderRadius: 6
                    }]
                };
                break;

            case 'pie':
            case 'donut':
                const colors = [primaryColor, accentColor, '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#06b6d4', '#ec4899'];
                chartData = {
                    labels: labels,
                    datasets: [{
                        data: values,
                        backgroundColor: colors.slice(0, values.length),
                        borderColor: '#ffffff',
                        borderWidth: 2
                    }]
                };
                break;

            case 'radar':
                chartData = {
                    labels: labels,
                    datasets: [{
                        label: reportConfig.datasetLabel || 'Metrics',
                        data: values,
                        backgroundColor: `${primaryColor}30`,
                        borderColor: primaryColor,
                        borderWidth: 2,
                        pointBackgroundColor: primaryColor,
                        pointBorderColor: '#fff',
                        pointHoverBackgroundColor: '#fff',
                        pointHoverBorderColor: primaryColor
                    }]
                };
                break;

            default:
                chartData = {
                    labels: labels,
                    datasets: [{
                        label: reportConfig.datasetLabel || 'Data',
                        data: values,
                        backgroundColor: primaryColor
                    }]
                };
        }

        // Chart options
        chartOptions = {
            responsive: false,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: chartType === 'pie' || chartType === 'donut' || chartType === 'radar',
                    position: 'bottom',
                    labels: {
                        color: '#1f2937',
                        font: { size: 12 },
                        padding: 15
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(255, 255, 255, 0.95)',
                    titleColor: '#1f2937',
                    bodyColor: '#1f2937',
                    borderColor: '#e5e7eb',
                    borderWidth: 1,
                    padding: 12
                }
            },
            animation: {
                duration: 0 // Disable animation for PDF generation
            }
        };

        // Add scales for appropriate chart types
        if (chartType !== 'pie' && chartType !== 'donut' && chartType !== 'radar') {
            chartOptions.scales = {
                x: {
                    ticks: { color: '#1f2937', font: { size: 11 } },
                    grid: { color: 'rgba(0, 0, 0, 0.1)', drawBorder: false }
                },
                y: {
                    beginAtZero: true,
                    ticks: { color: '#1f2937', font: { size: 11 }, precision: 0 },
                    grid: { color: 'rgba(0, 0, 0, 0.1)', drawBorder: false }
                }
            };
        }

        if (chartType === 'donut') {
            chartOptions.cutout = '65%';
        }

        // Create chart
        const chart = new Chart(canvas, {
            // type: chartType === 'donut' ? 'doughnut' : chartType,
            type: chartType === 'donut' ? 'doughnut' : chartType === 'area' ? 'line' : chartType,
            data: chartData,
            options: chartOptions
        });

        // Wait for chart to render
        await new Promise(resolve => setTimeout(resolve, 500));

        // Generate PDF
        console.log('Generating PDF from report preview...');

        const reportPreview = document.getElementById('reportPreview');

        // Use html2canvas to capture the report
        const canvas2 = await html2canvas(reportPreview, {
            scale: 2,
            useCORS: true,
            logging: false,
            backgroundColor: '#ffffff',
            width: 1200,
            windowWidth: 1200
        });

        // Create PDF with proper margins
        const { jsPDF } = window.jspdf;
        const pdf = new jsPDF({
            orientation: 'portrait',
            unit: 'mm',
            format: 'a4'
        });

        const imgData = canvas2.toDataURL('image/png', 1.0);

        // Define margins (in mm)
        const marginTop = 10;
        const marginBottom = 10;
        const marginLeft = 10;
        const marginRight = 10;

        // Calculate usable page dimensions
        const pageWidth = pdf.internal.pageSize.getWidth();
        const pageHeight = pdf.internal.pageSize.getHeight();
        const usableWidth = pageWidth - marginLeft - marginRight;
        const usableHeight = pageHeight - marginTop - marginBottom;

        // Calculate image dimensions to fit within margins
        const imgWidth = usableWidth;
        const imgHeight = (canvas2.height * usableWidth) / canvas2.width;

        // Calculate how many pages we need
        let heightLeft = imgHeight;
        let position = 0;
        let pageNumber = 1;

        // First page
        const sourceY = 0;
        const sourceHeight = Math.min(canvas2.height, (usableHeight * canvas2.width) / usableWidth);

        pdf.addImage(
            imgData,
            'PNG',
            marginLeft,  // x position with left margin
            marginTop,   // y position with top margin
            usableWidth, // width within margins
            Math.min(usableHeight, imgHeight), // height within margins
            undefined,
            'FAST',
            0
        );

        heightLeft -= usableHeight;

        // Additional pages if content exceeds one page
        while (heightLeft > 0) {
            pdf.addPage();
            pageNumber++;

            // Calculate the portion of the original image to use
            const yOffset = usableHeight * (pageNumber - 1);

            pdf.addImage(
                imgData,
                'PNG',
                marginLeft,
                marginTop - yOffset, // Offset to show next portion
                usableWidth,
                imgHeight,
                undefined,
                'FAST',
                0
            );

            heightLeft -= usableHeight;
        }

        // Download PDF
        const filename = reportConfig.filename || `Report_${new Date().toISOString().split('T')[0]}.pdf`;
        pdf.save(filename);

        console.log('PDF generated and downloaded successfully');

        // Clean up chart
        if (chart) {
            chart.destroy();
        }

        return true;

    } catch (error) {
        console.error('Error generating professional report:', error);
        return false;
    }
};

// Detailed Comparison Report Generator
window.generateComparisonReport = async function(reportConfig) {
    try {
        console.log('Generating detailed comparison report...', reportConfig);
        
        const { jsPDF } = window.jspdf;
        const pdf = new jsPDF({
            orientation: 'portrait',
            unit: 'mm',
            format: 'a4'
        });
        
        const pageWidth = pdf.internal.pageSize.getWidth();
        const pageHeight = pdf.internal.pageSize.getHeight();
        const margin = 15;
        const contentWidth = pageWidth - (margin * 2);
        let yPos = margin;
        
        const primaryColor = reportConfig.primaryColor || '#3b82f6';
        const accentColor = reportConfig.accentColor || '#8b5cf6';
        
        // Helper function to add new page if needed
        const checkPageBreak = (requiredSpace) => {
            if (yPos + requiredSpace > pageHeight - margin) {
                pdf.addPage();
                yPos = margin;
                return true;
            }
            return false;
        };
        
        // Helper function to draw a colored box
        const drawBox = (x, y, w, h, color, radius = 3) => {
            pdf.setFillColor(color);
            pdf.roundedRect(x, y, w, h, radius, radius, 'F');
        };
        
        // ========== HEADER SECTION ==========
        // Header background
        const headerGradient = pdf.setFillColor(59, 130, 246);
        pdf.rect(0, 0, pageWidth, 45, 'F');
        
        // Title
        pdf.setTextColor(255, 255, 255);
        pdf.setFontSize(22);
        pdf.setFont('helvetica', 'bold');
        pdf.text(reportConfig.title || 'Comparison Report', margin, 20);
        
        // Subtitle
        pdf.setFontSize(11);
        pdf.setFont('helvetica', 'normal');
        pdf.text(reportConfig.subtitle || '', margin, 30);
        
        // Date
        pdf.setFontSize(9);
        pdf.text(reportConfig.dateRange || '', margin, 38);
        
        yPos = 55;
        
        // ========== SUMMARY SECTION ==========
        pdf.setTextColor(31, 41, 55);
        pdf.setFontSize(14);
        pdf.setFont('helvetica', 'bold');
        pdf.text('SUMMARY', margin, yPos);
        yPos += 8;
        
        const summary = reportConfig.summary || {};
        const boxWidth = (contentWidth - 6) / 4;
        const boxHeight = 25;
        
        // Helper function to convert hex to RGB
        const hexToRgb = (hex) => {
            const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
            return result ? {
                r: parseInt(result[1], 16),
                g: parseInt(result[2], 16),
                b: parseInt(result[3], 16)
            } : { r: 200, g: 200, b: 200 };
        };
        
        // Summary boxes with RGB colors
        const summaryBoxes = [
            { label: 'Baseline (Period A)', value: summary.periodATotal || 0, bgColor: { r: 219, g: 234, b: 254 }, textColor: { r: 30, g: 64, b: 175 }, subtext: summary.periodALabel },
            { label: 'Current (Period B)', value: summary.periodBTotal || 0, bgColor: { r: 220, g: 252, b: 231 }, textColor: { r: 22, g: 101, b: 52 }, subtext: summary.periodBLabel },
            { label: 'Change', value: (summary.absoluteChange >= 0 ? '+' : '') + (summary.absoluteChange || 0), bgColor: summary.absoluteChange >= 0 ? { r: 209, g: 250, b: 229 } : { r: 254, g: 226, b: 226 }, textColor: summary.absoluteChange >= 0 ? { r: 5, g: 150, b: 105 } : { r: 220, g: 38, b: 38 } },
            { label: '% Change', value: (summary.percentageChange >= 0 ? '+' : '') + (summary.percentageChange || 0) + '%', bgColor: summary.percentageChange >= 0 ? { r: 209, g: 250, b: 229 } : { r: 254, g: 226, b: 226 }, textColor: summary.percentageChange >= 0 ? { r: 5, g: 150, b: 105 } : { r: 220, g: 38, b: 38 } }
        ];
        
        summaryBoxes.forEach((box, index) => {
            const x = margin + (index * (boxWidth + 2));
            
            // Box background with RGB values
            pdf.setFillColor(box.bgColor.r, box.bgColor.g, box.bgColor.b);
            pdf.roundedRect(x, yPos, boxWidth, boxHeight, 2, 2, 'F');
            
            // Label
            pdf.setTextColor(107, 114, 128);
            pdf.setFontSize(8);
            pdf.setFont('helvetica', 'normal');
            pdf.text(box.label, x + 3, yPos + 6);
            
            // Value with RGB values
            pdf.setTextColor(box.textColor.r, box.textColor.g, box.textColor.b);
            pdf.setFontSize(16);
            pdf.setFont('helvetica', 'bold');
            pdf.text(String(box.value), x + 3, yPos + 16);
            
            // Subtext if exists
            if (box.subtext) {
                pdf.setTextColor(107, 114, 128);
                pdf.setFontSize(7);
                pdf.setFont('helvetica', 'normal');
                pdf.text(box.subtext, x + 3, yPos + 22);
            }
        });
        
        yPos += boxHeight + 12;
        
        // ========== DAY-BY-DAY BREAKDOWN ==========
        const breakdown = reportConfig.breakdown || [];
        if (breakdown.length > 0) {
            checkPageBreak(50);
            
            pdf.setTextColor(31, 41, 55);
            pdf.setFontSize(14);
            pdf.setFont('helvetica', 'bold');
            pdf.text('DAY-BY-DAY BREAKDOWN', margin, yPos);
            yPos += 8;
            
            // Table header
            const colWidths = [contentWidth * 0.3, contentWidth * 0.17, contentWidth * 0.17, contentWidth * 0.18, contentWidth * 0.18];
            const headers = ['Date/Label', 'Period A', 'Period B', 'Difference', '% Change'];
            
            pdf.setFillColor(243, 244, 246);
            pdf.rect(margin, yPos, contentWidth, 8, 'F');
            
            pdf.setTextColor(75, 85, 99);
            pdf.setFontSize(9);
            pdf.setFont('helvetica', 'bold');
            
            let xPos = margin + 2;
            headers.forEach((header, i) => {
                pdf.text(header, xPos, yPos + 5.5);
                xPos += colWidths[i];
            });
            yPos += 10;
            
            // Table rows - show all items
            pdf.setFont('helvetica', 'normal');
            breakdown.forEach((row, index) => {
                checkPageBreak(8);
                
                if (index % 2 === 0) {
                    pdf.setFillColor(249, 250, 251);
                    pdf.rect(margin, yPos - 2, contentWidth, 7, 'F');
                }
                
                pdf.setTextColor(31, 41, 55);
                pdf.setFontSize(8);
                
                xPos = margin + 2;
                pdf.text(String(row.label || ''), xPos, yPos + 3);
                xPos += colWidths[0];
                pdf.text(String(row.periodA || 0), xPos, yPos + 3);
                xPos += colWidths[1];
                pdf.text(String(row.periodB || 0), xPos, yPos + 3);
                xPos += colWidths[2];
                
                // Difference with color
                const diff = row.difference || 0;
                if (diff >= 0) {
                    pdf.setTextColor(22, 163, 74); // Green
                } else {
                    pdf.setTextColor(220, 38, 38); // Red
                }
                pdf.text((diff >= 0 ? '+' : '') + String(diff), xPos, yPos + 3);
                xPos += colWidths[3];
                
                // Percent change with color
                const pct = row.percentChange || 0;
                if (pct >= 0) {
                    pdf.setTextColor(22, 163, 74); // Green
                } else {
                    pdf.setTextColor(220, 38, 38); // Red
                }
                pdf.text((pct >= 0 ? '+' : '') + String(pct) + '%', xPos, yPos + 3);
                
                yPos += 7;
            });
            
            // Total row
            pdf.setFillColor(59, 130, 246);
            pdf.rect(margin, yPos - 2, contentWidth, 8, 'F');
            pdf.setTextColor(255, 255, 255);
            pdf.setFont('helvetica', 'bold');
            pdf.setFontSize(9);
            
            xPos = margin + 2;
            pdf.text('TOTAL', xPos, yPos + 4);
            xPos += colWidths[0];
            pdf.text(String(summary.periodATotal || 0), xPos, yPos + 4);
            xPos += colWidths[1];
            pdf.text(String(summary.periodBTotal || 0), xPos, yPos + 4);
            xPos += colWidths[2];
            const totalDiff = summary.absoluteChange || 0;
            pdf.text((totalDiff >= 0 ? '+' : '') + String(totalDiff), xPos, yPos + 4);
            xPos += colWidths[3];
            const totalPct = summary.percentageChange || 0;
            pdf.text((totalPct >= 0 ? '+' : '') + String(totalPct) + '%', xPos, yPos + 4);
            
            yPos += 15;
        }
        
        // ========== NEW ITEMS SECTION ==========
        const newItems = reportConfig.newItems || [];
        if (newItems.length > 0) {
            checkPageBreak(40);
            
            pdf.setTextColor(31, 41, 55);
            pdf.setFontSize(14);
            pdf.setFont('helvetica', 'bold');
            const newItemsTitle = `NEW ${(reportConfig.itemTypeName || 'Items').toUpperCase()} IN CURRENT PERIOD`;
            pdf.text(newItemsTitle, margin, yPos);
            
            // Badge - positioned next to the title text, centered vertically
            const titleWidth = pdf.getTextWidth(newItemsTitle);
            const badgeX = margin + titleWidth + 5;
            const badgeWidth = 22;
            const badgeHeight = 7;
            const badgeY = yPos - 6; // Position badge to align with text baseline
            
            pdf.setFillColor(34, 197, 94);
            pdf.roundedRect(badgeX, badgeY, badgeWidth, badgeHeight, 2, 2, 'F');
            
            // Badge text - centered inside the badge
            pdf.setTextColor(255, 255, 255);
            pdf.setFontSize(9);
            pdf.setFont('helvetica', 'bold');
            const badgeText = `+${newItems.length}`;
            const badgeTextWidth = pdf.getTextWidth(badgeText);
            pdf.text(badgeText, badgeX + (badgeWidth - badgeTextWidth) / 2, badgeY + 5);
            
            yPos += 8;
            
            // Table header
            const itemColWidths = [contentWidth * 0.25, contentWidth * 0.30, contentWidth * 0.15, contentWidth * 0.15, contentWidth * 0.15];
            const itemHeaders = ['Name', 'Email', 'Role', 'Action', 'Date'];
            
            pdf.setFillColor(220, 252, 231);
            pdf.rect(margin, yPos, contentWidth, 7, 'F');
            
            pdf.setTextColor(22, 101, 52);
            pdf.setFontSize(8);
            pdf.setFont('helvetica', 'bold');
            
            xPos = margin + 2;
            itemHeaders.forEach((header, i) => {
                pdf.text(header, xPos, yPos + 5);
                xPos += itemColWidths[i];
            });
            yPos += 9;
            
            // Rows - show all items
            pdf.setFont('helvetica', 'normal');
            pdf.setTextColor(31, 41, 55);
            newItems.forEach((item, index) => {
                checkPageBreak(7);
                
                if (index % 2 === 0) {
                    pdf.setFillColor(240, 253, 244);
                    pdf.rect(margin, yPos - 2, contentWidth, 6, 'F');
                }
                
                pdf.setFontSize(7);
                xPos = margin + 2;
                pdf.text(String(item.name || '').substring(0, 25), xPos, yPos + 2);
                xPos += itemColWidths[0];
                pdf.text(String(item.email || '').substring(0, 30), xPos, yPos + 2);
                xPos += itemColWidths[1];
                pdf.text(String(item.role || ''), xPos, yPos + 2);
                xPos += itemColWidths[2];
                pdf.text(String(item.action || ''), xPos, yPos + 2);
                xPos += itemColWidths[3];
                pdf.text(String(item.date || ''), xPos, yPos + 2);
                
                yPos += 6;
            });
            
            yPos += 8;
        }
        
        // ========== REMOVED ITEMS SECTION ==========
        const removedItems = reportConfig.removedItems || [];
        if (removedItems.length > 0) {
            checkPageBreak(40);
            
            pdf.setTextColor(31, 41, 55);
            pdf.setFontSize(14);
            pdf.setFont('helvetica', 'bold');
            const removedItemsTitle = `REMOVED ${(reportConfig.itemTypeName || 'Items').toUpperCase()}`;
            pdf.text(removedItemsTitle, margin, yPos);
            
            // Badge
            pdf.setFillColor(239, 68, 68);
            pdf.roundedRect(margin + pdf.getTextWidth(removedItemsTitle) + 5, yPos - 5, 20, 7, 2, 2, 'F');
            pdf.setTextColor(255, 255, 255);
            pdf.setFontSize(8);
            pdf.text(`-${removedItems.length}`, margin + pdf.getTextWidth(removedItemsTitle) + 10, yPos - 0.5);
            
            yPos += 8;
            
            // Table header
            const itemColWidths = [contentWidth * 0.25, contentWidth * 0.30, contentWidth * 0.15, contentWidth * 0.15, contentWidth * 0.15];
            const itemHeaders = ['Name', 'Email', 'Role', 'Action', 'Date'];
            
            pdf.setFillColor(254, 226, 226);
            pdf.rect(margin, yPos, contentWidth, 7, 'F');
            
            pdf.setTextColor(153, 27, 27);
            pdf.setFontSize(8);
            pdf.setFont('helvetica', 'bold');
            
            xPos = margin + 2;
            itemHeaders.forEach((header, i) => {
                pdf.text(header, xPos, yPos + 5);
                xPos += itemColWidths[i];
            });
            yPos += 9;
            
            // Rows - show all items
            pdf.setFont('helvetica', 'normal');
            pdf.setTextColor(31, 41, 55);
            removedItems.forEach((item, index) => {
                checkPageBreak(7);
                
                if (index % 2 === 0) {
                    pdf.setFillColor(254, 242, 242);
                    pdf.rect(margin, yPos - 2, contentWidth, 6, 'F');
                }
                
                pdf.setFontSize(7);
                xPos = margin + 2;
                pdf.text(String(item.name || '').substring(0, 25), xPos, yPos + 2);
                xPos += itemColWidths[0];
                pdf.text(String(item.email || '').substring(0, 30), xPos, yPos + 2);
                xPos += itemColWidths[1];
                pdf.text(String(item.role || ''), xPos, yPos + 2);
                xPos += itemColWidths[2];
                pdf.text(String(item.action || ''), xPos, yPos + 2);
                xPos += itemColWidths[3];
                pdf.text(String(item.date || ''), xPos, yPos + 2);
                
                yPos += 6;
            });
            
            yPos += 8;
        }
        
        // ========== ALL CURRENT PERIOD ITEMS ==========
        const allCurrentItems = reportConfig.allCurrentItems || [];
        if (allCurrentItems.length > 0) {
            checkPageBreak(40);
            
            pdf.setTextColor(31, 41, 55);
            pdf.setFontSize(14);
            pdf.setFont('helvetica', 'bold');
            const allItemsTitle = `ALL ${(reportConfig.itemTypeName || 'Items').toUpperCase()} IN CURRENT PERIOD`;
            pdf.text(allItemsTitle, margin, yPos);
            
            // Badge - positioned next to the title text, centered vertically
            const allTitleWidth = pdf.getTextWidth(allItemsTitle);
            const allBadgeX = margin + allTitleWidth + 5;
            const allBadgeText = `${allCurrentItems.length} total`;
            pdf.setFontSize(9);
            const allBadgeTextWidth = pdf.getTextWidth(allBadgeText);
            const allBadgeWidth = allBadgeTextWidth + 8;
            const allBadgeHeight = 7;
            const allBadgeY = yPos - 6;
            
            pdf.setFillColor(59, 130, 246);
            pdf.roundedRect(allBadgeX, allBadgeY, allBadgeWidth, allBadgeHeight, 2, 2, 'F');
            
            // Badge text - centered inside the badge
            pdf.setTextColor(255, 255, 255);
            pdf.setFont('helvetica', 'bold');
            pdf.text(allBadgeText, allBadgeX + 4, allBadgeY + 5);
            
            yPos += 8;
            
            // Table header
            const itemColWidths = [contentWidth * 0.22, contentWidth * 0.28, contentWidth * 0.12, contentWidth * 0.12, contentWidth * 0.16, contentWidth * 0.10];
            const itemHeaders = ['Name', 'Email', 'Role', 'Action', 'Date', 'Status'];
            
            pdf.setFillColor(219, 234, 254);
            pdf.rect(margin, yPos, contentWidth, 7, 'F');
            
            pdf.setTextColor(30, 64, 175);
            pdf.setFontSize(8);
            pdf.setFont('helvetica', 'bold');
            
            xPos = margin + 2;
            itemHeaders.forEach((header, i) => {
                pdf.text(header, xPos, yPos + 5);
                xPos += itemColWidths[i];
            });
            yPos += 9;
            
            // Rows - show all items
            pdf.setFont('helvetica', 'normal');
            allCurrentItems.forEach((item, index) => {
                checkPageBreak(7);
                
                if (index % 2 === 0) {
                    pdf.setFillColor(239, 246, 255);
                    pdf.rect(margin, yPos - 2, contentWidth, 6, 'F');
                }
                
                pdf.setTextColor(31, 41, 55);
                pdf.setFontSize(7);
                xPos = margin + 2;
                pdf.text(String(item.name || '').substring(0, 22), xPos, yPos + 2);
                xPos += itemColWidths[0];
                pdf.text(String(item.email || '').substring(0, 28), xPos, yPos + 2);
                xPos += itemColWidths[1];
                pdf.text(String(item.role || ''), xPos, yPos + 2);
                xPos += itemColWidths[2];
                pdf.text(String(item.action || ''), xPos, yPos + 2);
                xPos += itemColWidths[3];
                pdf.text(String(item.date || ''), xPos, yPos + 2);
                xPos += itemColWidths[4];
                
                // New badge
                if (item.isNew) {
                    pdf.setFillColor(34, 197, 94);
                    pdf.roundedRect(xPos, yPos - 1, 12, 4, 1, 1, 'F');
                    pdf.setTextColor(255, 255, 255);
                    pdf.setFontSize(6);
                    pdf.text('NEW', xPos + 2, yPos + 1.5);
                } else {
                    pdf.setTextColor(107, 114, 128);
                    pdf.setFontSize(6);
                    pdf.text('Existing', xPos, yPos + 2);
                }
                
                yPos += 6;
            });
            
            yPos += 8;
        }
        
        // ========== RECOMMENDATIONS ==========
        const recommendations = reportConfig.recommendations || [];
        if (recommendations.length > 0) {
            checkPageBreak(40);
            
            pdf.setTextColor(31, 41, 55);
            pdf.setFontSize(14);
            pdf.setFont('helvetica', 'bold');
            pdf.text('RECOMMENDATIONS', margin, yPos);
            yPos += 8;
            
            recommendations.slice(0, 3).forEach(recItem => {
                // Handle recommendation as string or object
                const rec = typeof recItem === 'string' ? { title: recItem, description: '', priority: 'low' } : recItem;
                checkPageBreak(20);
                
                // Use RGB values directly instead of hex strings
                const priorityColors = {
                    high: { bg: [254, 242, 242], border: [239, 68, 68], text: [153, 27, 27] },
                    medium: { bg: [255, 251, 235], border: [245, 158, 11], text: [146, 64, 14] },
                    low: { bg: [240, 253, 244], border: [34, 197, 94], text: [22, 101, 52] }
                };
                const colors = priorityColors[rec.priority] || priorityColors.low;
                
                pdf.setFillColor(...colors.bg);
                pdf.roundedRect(margin, yPos, contentWidth, 15, 2, 2, 'F');
                
                // Priority badge
                pdf.setFillColor(...colors.border);
                pdf.roundedRect(margin + contentWidth - 25, yPos + 2, 22, 5, 1, 1, 'F');
                pdf.setTextColor(255, 255, 255);
                pdf.setFontSize(7);
                pdf.text((rec.priority || 'low').toUpperCase(), margin + contentWidth - 23, yPos + 5.5);
                
                // Title
                pdf.setTextColor(...colors.text);
                pdf.setFontSize(10);
                pdf.setFont('helvetica', 'bold');
                pdf.text(rec.title || '', margin + 4, yPos + 6);
                
                // Description
                pdf.setFont('helvetica', 'normal');
                pdf.setFontSize(8);
                pdf.setTextColor(75, 85, 99);
                const descText = rec.description || '';
                pdf.text(descText.substring(0, 100) + (descText.length > 100 ? '...' : ''), margin + 4, yPos + 12);
                
                yPos += 18;
            });
        }
        
        // ========== FOOTER ==========
        const totalPages = pdf.internal.getNumberOfPages();
        for (let i = 1; i <= totalPages; i++) {
            pdf.setPage(i);
            
            // Footer line
            pdf.setDrawColor(229, 231, 235);
            pdf.line(margin, pageHeight - 12, pageWidth - margin, pageHeight - 12);
            
            // Footer text
            pdf.setTextColor(156, 163, 175);
            pdf.setFontSize(8);
            pdf.setFont('helvetica', 'normal');
            pdf.text('Generated by MediChat AI', margin, pageHeight - 7);
            pdf.text(`Page ${i} of ${totalPages}`, pageWidth - margin - 20, pageHeight - 7);
        }
        
        // Save PDF
        const filename = `${reportConfig.reportType || 'comparison'}_report_${new Date().toISOString().split('T')[0]}.pdf`;
        pdf.save(filename);
        
        console.log('Comparison report PDF generated successfully');
        return true;
        
    } catch (error) {
        console.error('Error generating comparison report:', error);
        return false;
    }
};

// Generate PDF for preview/sharing (returns data URL) - Uses same logic as generateComparisonReport
window.generateComparisonReportPreview = function(reportConfig) {
    try {
        const { jsPDF } = window.jspdf;
        const pdf = new jsPDF({
            orientation: 'portrait',
            unit: 'mm',
            format: 'a4'
        });
        
        const pageWidth = pdf.internal.pageSize.getWidth();
        const pageHeight = pdf.internal.pageSize.getHeight();
        const margin = 15;
        const contentWidth = pageWidth - (2 * margin);
        let yPos = 20;
        
        // Helper function to check page break
        const checkPageBreak = (requiredHeight) => {
            if (yPos + requiredHeight > pageHeight - 20) {
                pdf.addPage();
                yPos = 20;
                return true;
            }
            return false;
        };
        
        // Helper function to convert hex to RGB
        const hexToRgb = (hex) => {
            const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
            return result ? {
                r: parseInt(result[1], 16),
                g: parseInt(result[2], 16),
                b: parseInt(result[3], 16)
            } : { r: 0, g: 0, b: 0 };
        };
        
        // ========== HEADER ==========
        pdf.setFillColor(59, 130, 246);
        pdf.rect(0, 0, pageWidth, 45, 'F');
        
        pdf.setTextColor(255, 255, 255);
        pdf.setFontSize(22);
        pdf.setFont('helvetica', 'bold');
        pdf.text(reportConfig.title || 'Comparison Report', margin, 20);
        
        pdf.setFontSize(11);
        pdf.setFont('helvetica', 'normal');
        pdf.text(reportConfig.subtitle || '', margin, 30);
        
        pdf.setFontSize(9);
        pdf.text(reportConfig.dateRange || '', margin, 38);
        
        yPos = 60;
        
        // ========== SUMMARY SECTION ==========
        const summary = reportConfig.summary || {};
        
        pdf.setTextColor(31, 41, 55);
        pdf.setFontSize(14);
        pdf.setFont('helvetica', 'bold');
        pdf.text('SUMMARY', margin, yPos);
        yPos += 10;
        
        // Summary boxes
        const boxWidth = (contentWidth - 6) / 4;
        const boxHeight = 25;
        
        const boxes = [
            { label: 'Baseline (Period A)', value: String(summary.periodATotal || 0), subtext: summary.periodALabel || '', bgColor: '#dbeafe', textColor: '#1e40af' },
            { label: 'Current (Period B)', value: String(summary.periodBTotal || 0), subtext: summary.periodBLabel || '', bgColor: '#dcfce7', textColor: '#166534' },
            { label: 'Change', value: (summary.absoluteChange >= 0 ? '+' : '') + String(summary.absoluteChange || 0), subtext: 'Increase', bgColor: '#d1fae5', textColor: '#059669' },
            { label: '% Change', value: (summary.percentageChange >= 0 ? '+' : '') + String(summary.percentageChange || 0) + '%', subtext: 'Growth', bgColor: '#d1fae5', textColor: '#059669' }
        ];
        
        boxes.forEach((box, idx) => {
            const x = margin + (idx * (boxWidth + 2));
            
            const bgRgb = hexToRgb(box.bgColor);
            pdf.setFillColor(bgRgb.r, bgRgb.g, bgRgb.b);
            pdf.roundedRect(x, yPos, boxWidth, boxHeight, 2, 2, 'F');
            
            pdf.setTextColor(107, 114, 128);
            pdf.setFontSize(8);
            pdf.setFont('helvetica', 'normal');
            pdf.text(box.label, x + 3, yPos + 6);
            
            const textRgb = hexToRgb(box.textColor);
            pdf.setTextColor(textRgb.r, textRgb.g, textRgb.b);
            pdf.setFontSize(16);
            pdf.setFont('helvetica', 'bold');
            pdf.text(box.value, x + 3, yPos + 16);
            
            pdf.setTextColor(107, 114, 128);
            pdf.setFontSize(7);
            pdf.setFont('helvetica', 'normal');
            const subtext = (box.subtext || '').substring(0, 22);
            pdf.text(subtext, x + 3, yPos + 22);
        });
        
        yPos += boxHeight + 15;
        
        // ========== DAY-BY-DAY BREAKDOWN ==========
        const breakdown = reportConfig.breakdown || [];
        if (breakdown.length > 0) {
            checkPageBreak(50);
            
            pdf.setTextColor(31, 41, 55);
            pdf.setFontSize(14);
            pdf.setFont('helvetica', 'bold');
            pdf.text('DAY-BY-DAY BREAKDOWN', margin, yPos);
            yPos += 8;
            
            // Table header
            pdf.setFillColor(243, 244, 246);
            pdf.rect(margin, yPos, contentWidth, 8, 'F');
            
            pdf.setTextColor(75, 85, 99);
            pdf.setFontSize(9);
            pdf.setFont('helvetica', 'bold');
            
            const cols = [margin + 2, margin + 56, margin + 100, margin + 147, margin + 200];
            pdf.text('Date/Label', cols[0], yPos + 5.5);
            pdf.text('Period A', cols[1], yPos + 5.5);
            pdf.text('Period B', cols[2], yPos + 5.5);
            pdf.text('Difference', cols[3], yPos + 5.5);
            pdf.text('% Change', cols[4], yPos + 5.5);
            
            yPos += 8;
            
            // Table rows - show all items
            breakdown.forEach((row, idx) => {
                checkPageBreak(7);
                
                if (idx % 2 === 0) {
                    pdf.setFillColor(249, 250, 251);
                    pdf.rect(margin, yPos, contentWidth, 7, 'F');
                }
                
                pdf.setTextColor(31, 41, 55);
                pdf.setFontSize(8);
                pdf.setFont('helvetica', 'normal');
                pdf.text(String(row.label || ''), cols[0], yPos + 5);
                pdf.text(String(row.periodA || 0), cols[1], yPos + 5);
                pdf.text(String(row.periodB || 0), cols[2], yPos + 5);
                
                const diff = row.difference || 0;
                if (diff >= 0) {
                    pdf.setTextColor(22, 163, 74);
                } else {
                    pdf.setTextColor(220, 38, 38);
                }
                pdf.text((diff >= 0 ? '+' : '') + String(diff), cols[3], yPos + 5);
                
                const pct = row.percentChange || 0;
                pdf.text((pct >= 0 ? '+' : '') + String(pct) + '%', cols[4], yPos + 5);
                
                yPos += 7;
            });
            
            // Total row
            pdf.setFillColor(59, 130, 246);
            pdf.rect(margin, yPos, contentWidth, 8, 'F');
            
            pdf.setTextColor(255, 255, 255);
            pdf.setFontSize(9);
            pdf.setFont('helvetica', 'bold');
            pdf.text('TOTAL', cols[0], yPos + 5.5);
            pdf.text(String(summary.periodATotal || 0), cols[1], yPos + 5.5);
            pdf.text(String(summary.periodBTotal || 0), cols[2], yPos + 5.5);
            pdf.text((summary.absoluteChange >= 0 ? '+' : '') + String(summary.absoluteChange || 0), cols[3], yPos + 5.5);
            pdf.text((summary.percentageChange >= 0 ? '+' : '') + String(summary.percentageChange || 0) + '%', cols[4], yPos + 5.5);
            
            yPos += 15;
        }
        
        // ========== NEW ITEMS ==========
        const newItems = reportConfig.newItems || [];
        if (newItems.length > 0) {
            checkPageBreak(40);
            
            pdf.setTextColor(31, 41, 55);
            pdf.setFontSize(14);
            pdf.setFont('helvetica', 'bold');
            pdf.text(`NEW ${reportConfig.itemTypeName?.toUpperCase() || 'ITEMS'} IN CURRENT PERIOD`, margin, yPos);
            
            // Badge
            pdf.setFillColor(34, 197, 94);
            pdf.roundedRect(margin + 155, yPos - 6, 20, 7, 2, 2, 'F');
            pdf.setTextColor(255, 255, 255);
            pdf.setFontSize(8);
            pdf.setFont('helvetica', 'bold');
            pdf.text(`+${newItems.length}`, margin + 158, yPos - 1);
            
            yPos += 8;
            
            // Table header
            pdf.setFillColor(220, 252, 231);
            pdf.rect(margin, yPos, contentWidth, 7, 'F');
            
            pdf.setTextColor(22, 101, 52);
            pdf.setFontSize(8);
            pdf.setFont('helvetica', 'bold');
            
            const newCols = [margin + 2, margin + 47, margin + 101, margin + 128, margin + 155];
            pdf.text('Name', newCols[0], yPos + 5);
            pdf.text('Email', newCols[1], yPos + 5);
            pdf.text('Role', newCols[2], yPos + 5);
            pdf.text('Action', newCols[3], yPos + 5);
            pdf.text('Date', newCols[4], yPos + 5);
            
            yPos += 7;
            
            // Table rows - show all items
            newItems.forEach((item, idx) => {
                checkPageBreak(6);
                
                if (idx % 2 === 0) {
                    pdf.setFillColor(240, 253, 244);
                    pdf.rect(margin, yPos, contentWidth, 6, 'F');
                }
                
                pdf.setTextColor(31, 41, 55);
                pdf.setFontSize(7);
                pdf.setFont('helvetica', 'normal');
                pdf.text((item.name || '').substring(0, 18), newCols[0], yPos + 4);
                pdf.text((item.email || '').substring(0, 28), newCols[1], yPos + 4);
                pdf.text((item.role || '').substring(0, 10), newCols[2], yPos + 4);
                pdf.text((item.action || '').substring(0, 10), newCols[3], yPos + 4);
                pdf.text((item.date || '').substring(0, 18), newCols[4], yPos + 4);
                
                yPos += 6;
            });
            
            yPos += 10;
        }
        
        // ========== RECOMMENDATIONS ==========
        const recommendations = reportConfig.recommendations || [];
        if (recommendations.length > 0) {
            checkPageBreak(40);
            
            pdf.setTextColor(31, 41, 55);
            pdf.setFontSize(14);
            pdf.setFont('helvetica', 'bold');
            pdf.text('RECOMMENDATIONS', margin, yPos);
            yPos += 8;
            
            recommendations.forEach(rec => {
                checkPageBreak(20);
                
                const priorityColors = {
                    high: { bg: [254, 242, 242], border: [239, 68, 68], text: [153, 27, 27] },
                    medium: { bg: [255, 251, 235], border: [245, 158, 11], text: [146, 64, 14] },
                    low: { bg: [240, 253, 244], border: [34, 197, 94], text: [22, 101, 52] }
                };
                const colors = priorityColors[rec.priority] || priorityColors.low;
                
                pdf.setFillColor(...colors.bg);
                pdf.roundedRect(margin, yPos, contentWidth, 15, 2, 2, 'F');
                
                // Priority badge
                pdf.setFillColor(...colors.border);
                pdf.roundedRect(margin + contentWidth - 25, yPos + 2, 22, 5, 1, 1, 'F');
                pdf.setTextColor(255, 255, 255);
                pdf.setFontSize(7);
                pdf.setFont('helvetica', 'bold');
                pdf.text((rec.priority || 'low').toUpperCase(), margin + contentWidth - 23, yPos + 5.5);
                
                // Title
                pdf.setTextColor(...colors.text);
                pdf.setFontSize(10);
                pdf.setFont('helvetica', 'bold');
                pdf.text(rec.title || '', margin + 4, yPos + 6);
                
                // Description
                pdf.setTextColor(75, 85, 99);
                pdf.setFontSize(8);
                pdf.setFont('helvetica', 'normal');
                const descText = rec.description || '';
                pdf.text(descText.substring(0, 100) + (descText.length > 100 ? '...' : ''), margin + 4, yPos + 12);
                
                yPos += 18;
            });
        }
        
        // ========== FOOTER ==========
        const totalPages = pdf.internal.getNumberOfPages();
        for (let i = 1; i <= totalPages; i++) {
            pdf.setPage(i);
            
            pdf.setDrawColor(229, 231, 235);
            pdf.line(margin, pageHeight - 12, pageWidth - margin, pageHeight - 12);
            
            pdf.setTextColor(156, 163, 175);
            pdf.setFontSize(8);
            pdf.setFont('helvetica', 'normal');
            pdf.text('Generated by MediChat AI', margin, pageHeight - 7);
            pdf.text(`Page ${i} of ${totalPages}`, pageWidth - margin - 20, pageHeight - 7);
        }
        
        // Return as data URL for iframe preview
        return pdf.output('datauristring');
        
    } catch (error) {
        console.error('Error generating preview:', error);
        return null;
    }
};

// Share PDF via Web Share API or download
window.sharePdfReport = async function(reportConfig, method) {
    try {
        const { jsPDF } = window.jspdf;
        const pdf = new jsPDF({
            orientation: 'portrait',
            unit: 'mm',
            format: 'a4'
        });
        
        // Generate a simplified PDF for sharing
        const pageWidth = pdf.internal.pageSize.getWidth();
        const margin = 15;
        
        // Header
        pdf.setFillColor(59, 130, 246);
        pdf.rect(0, 0, pageWidth, 35, 'F');
        
        pdf.setTextColor(255, 255, 255);
        pdf.setFontSize(18);
        pdf.setFont('helvetica', 'bold');
        pdf.text(reportConfig.title || 'Comparison Report', margin, 15);
        
        pdf.setFontSize(10);
        pdf.setFont('helvetica', 'normal');
        pdf.text(reportConfig.subtitle || '', margin, 23);
        pdf.text(reportConfig.dateRange || '', margin, 30);
        
        // Summary
        let yPos = 50;
        const summary = reportConfig.summary || {};
        
        pdf.setTextColor(31, 41, 55);
        pdf.setFontSize(12);
        pdf.setFont('helvetica', 'bold');
        pdf.text(`Summary: ${summary.periodATotal || 0} -> ${summary.periodBTotal || 0} (${summary.absoluteChange >= 0 ? '+' : ''}${summary.absoluteChange || 0}, ${summary.percentageChange >= 0 ? '+' : ''}${summary.percentageChange || 0}%)`, margin, yPos);
        
        // Get PDF blob
        const pdfBlob = pdf.output('blob');
        const filename = `${reportConfig.reportType || 'comparison'}_report.pdf`;
        
        // Check if Web Share API is available and supports files
        if (method === 'native' && navigator.share && navigator.canShare) {
            const file = new File([pdfBlob], filename, { type: 'application/pdf' });
            const shareData = { files: [file], title: reportConfig.title };
            
            if (navigator.canShare(shareData)) {
                await navigator.share(shareData);
                return { success: true, message: 'Shared successfully!' };
            }
        }
        
        // Fallback: download the file
        const url = URL.createObjectURL(pdfBlob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
        
        return { success: true, message: 'PDF downloaded!' };
        
    } catch (error) {
        console.error('Error sharing PDF:', error);
        return { success: false, message: error.message };
    }
};

// Export CSV to XLSX format
window.exportCsvToXlsx = function(csvString, filename) {
    try {
        // Check if SheetJS is available
        if (typeof XLSX === 'undefined') {
            // Fallback: Just download as CSV with xlsx extension (will work in Excel)
            const blob = new Blob([csvString], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = filename.replace('.xlsx', '.csv');
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            URL.revokeObjectURL(url);
            console.log('SheetJS not available, exported as CSV');
            return;
        }
        
        // Parse CSV to workbook
        const rows = csvString.split('\n').map(row => {
            // Handle quoted values with commas
            const values = [];
            let inQuotes = false;
            let currentValue = '';
            
            for (let char of row) {
                if (char === '"') {
                    inQuotes = !inQuotes;
                } else if (char === ',' && !inQuotes) {
                    values.push(currentValue.trim());
                    currentValue = '';
                } else {
                    currentValue += char;
                }
            }
            values.push(currentValue.trim());
            return values;
        }).filter(row => row.length > 1 || row[0] !== '');
        
        const worksheet = XLSX.utils.aoa_to_sheet(rows);
        const workbook = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(workbook, worksheet, 'Data');
        
        // Write and download
        XLSX.writeFile(workbook, filename);
        console.log('XLSX file exported successfully');
        
    } catch (error) {
        console.error('Error exporting XLSX:', error);
        // Fallback to CSV
        const blob = new Blob([csvString], { type: 'text/csv' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename.replace('.xlsx', '.csv');
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    }
};

// Generate Analytics PDF Report
window.generateAnalyticsPdfReport = function(reportConfig) {
    try {
        const { jsPDF } = window.jspdf;
        const pdf = new jsPDF({
            orientation: 'portrait',
            unit: 'mm',
            format: 'a4'
        });
        
        const pageWidth = pdf.internal.pageSize.getWidth();
        const pageHeight = pdf.internal.pageSize.getHeight();
        const margin = 15;
        const contentWidth = pageWidth - (2 * margin);
        let yPos = 20;
        
        // Header with gradient effect
        pdf.setFillColor(59, 130, 246);
        pdf.rect(0, 0, pageWidth, 40, 'F');
        pdf.setFillColor(99, 102, 241);
        pdf.rect(0, 30, pageWidth, 10, 'F');
        
        pdf.setTextColor(255, 255, 255);
        pdf.setFontSize(22);
        pdf.setFont('helvetica', 'bold');
        pdf.text(reportConfig.title || 'Analytics Report', margin, 18);
        
        pdf.setFontSize(11);
        pdf.setFont('helvetica', 'normal');
        pdf.text(reportConfig.subtitle || '', margin, 28);
        pdf.setFontSize(9);
        pdf.text(reportConfig.dateRange || '', margin, 36);
        
        yPos = 55;
        
        // Stats Overview Section
        const stats = reportConfig.stats || {};
        
        pdf.setTextColor(31, 41, 55);
        pdf.setFontSize(14);
        pdf.setFont('helvetica', 'bold');
        pdf.text('OVERVIEW', margin, yPos);
        yPos += 10;
        
        // Stats boxes
        const statBoxWidth = (contentWidth - 6) / 4;
        const statItems = [
            { label: 'Total Users', value: String(stats.totalUsers || 0), color: '#3b82f6' },
            { label: 'Patients', value: String(stats.totalPatients || 0), color: '#22c55e' },
            { label: 'Doctors', value: String(stats.totalDoctors || 0), color: '#8b5cf6' },
            { label: 'Active Today', value: String(stats.activeUsersToday || 0), color: '#f59e0b' }
        ];
        
        statItems.forEach((stat, idx) => {
            const x = margin + (idx * (statBoxWidth + 2));
            pdf.setFillColor(248, 250, 252);
            pdf.roundedRect(x, yPos, statBoxWidth, 25, 2, 2, 'F');
            
            pdf.setTextColor(107, 114, 128);
            pdf.setFontSize(8);
            pdf.text(stat.label, x + 3, yPos + 6);
            
            const hexColor = stat.color;
            const r = parseInt(hexColor.slice(1, 3), 16);
            const g = parseInt(hexColor.slice(3, 5), 16);
            const b = parseInt(hexColor.slice(5, 7), 16);
            pdf.setTextColor(r, g, b);
            pdf.setFontSize(16);
            pdf.setFont('helvetica', 'bold');
            pdf.text(stat.value, x + 3, yPos + 18);
        });
        
        yPos += 40;
        
        // User Growth Section
        pdf.setTextColor(31, 41, 55);
        pdf.setFontSize(14);
        pdf.setFont('helvetica', 'bold');
        pdf.text('USER GROWTH TREND', margin, yPos);
        yPos += 8;
        
        const userGrowth = reportConfig.userGrowth || { labels: [], data: [] };
        if (userGrowth.labels && userGrowth.labels.length > 0) {
            pdf.setFillColor(239, 246, 255);
            pdf.roundedRect(margin, yPos, contentWidth, 30, 2, 2, 'F');
            
            pdf.setTextColor(59, 130, 246);
            pdf.setFontSize(9);
            pdf.setFont('helvetica', 'normal');
            
            const growthLabels = userGrowth.labels.slice(0, 7);
            const growthData = userGrowth.data.slice(0, 7);
            let xOffset = margin + 5;
            
            growthLabels.forEach((label, idx) => {
                pdf.text(`${label}: ${growthData[idx] || 0}`, xOffset, yPos + 15);
                xOffset += (contentWidth - 10) / 7;
            });
            
            yPos += 35;
        }
        
        // Activity Breakdown Section
        pdf.setTextColor(31, 41, 55);
        pdf.setFontSize(14);
        pdf.setFont('helvetica', 'bold');
        pdf.text('ACTIVITY BREAKDOWN', margin, yPos);
        yPos += 8;
        
        const activityData = reportConfig.activityBreakdown || { labels: [], data: [] };
        if (activityData.labels && activityData.labels.length > 0) {
            pdf.setFillColor(240, 253, 244);
            pdf.roundedRect(margin, yPos, contentWidth, 35, 2, 2, 'F');
            
            let actY = yPos + 8;
            const actLabels = activityData.labels.slice(0, 6);
            const actValues = activityData.data.slice(0, 6);
            
            actLabels.forEach((label, idx) => {
                pdf.setTextColor(22, 163, 74);
                pdf.setFontSize(9);
                pdf.setFont('helvetica', 'bold');
                pdf.text(`${label}:`, margin + 5 + (idx % 3) * 60, actY + (Math.floor(idx / 3) * 12));
                pdf.setFont('helvetica', 'normal');
                pdf.text(String(actValues[idx] || 0), margin + 35 + (idx % 3) * 60, actY + (Math.floor(idx / 3) * 12));
            });
            
            yPos += 45;
        }
        
        // Role Distribution Section
        pdf.setTextColor(31, 41, 55);
        pdf.setFontSize(14);
        pdf.setFont('helvetica', 'bold');
        pdf.text('ROLE DISTRIBUTION', margin, yPos);
        yPos += 8;
        
        const roleData = reportConfig.roleDistribution || { labels: [], data: [] };
        if (roleData.labels && roleData.labels.length > 0) {
            const roleColors = ['#3b82f6', '#22c55e', '#8b5cf6', '#f59e0b'];
            
            roleData.labels.forEach((label, idx) => {
                const color = roleColors[idx % roleColors.length];
                const r = parseInt(color.slice(1, 3), 16);
                const g = parseInt(color.slice(3, 5), 16);
                const b = parseInt(color.slice(5, 7), 16);
                
                pdf.setFillColor(r, g, b);
                pdf.roundedRect(margin + (idx * 45), yPos, 40, 20, 2, 2, 'F');
                
                pdf.setTextColor(255, 255, 255);
                pdf.setFontSize(8);
                pdf.setFont('helvetica', 'bold');
                pdf.text(label, margin + 3 + (idx * 45), yPos + 8);
                pdf.setFontSize(12);
                pdf.text(String(roleData.data[idx] || 0), margin + 3 + (idx * 45), yPos + 16);
            });
            
            yPos += 30;
        }
        
        // Footer
        pdf.setDrawColor(229, 231, 235);
        pdf.line(margin, pageHeight - 12, pageWidth - margin, pageHeight - 12);
        pdf.setTextColor(156, 163, 175);
        pdf.setFontSize(8);
        pdf.setFont('helvetica', 'normal');
        pdf.text('Generated by MediChat AI Analytics', margin, pageHeight - 7);
        pdf.text(new Date().toLocaleString(), pageWidth - margin - 40, pageHeight - 7);
        
        // Save PDF
        const filename = `analytics-report-${new Date().toISOString().split('T')[0]}.pdf`;
        pdf.save(filename);
        
        console.log('Analytics PDF report generated successfully');
        return true;
        
    } catch (error) {
        console.error('Error generating analytics PDF:', error);
        return false;
    }
};

console.log('Charts.js with export and report functionality loaded successfully');
