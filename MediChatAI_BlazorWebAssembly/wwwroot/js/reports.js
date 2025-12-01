// Reports.js - Chart rendering and export utilities for Doctor Reports

// Store chart instances to enable updates
const chartInstances = {};

/**
 * Render a Chart.js chart
 * @param {string} chartId - The ID of the canvas element
 * @param {string} chartType - Type of chart (line, bar, pie, doughnut, etc.)
 * @param {object} chartData - Chart data object
 * @param {object} chartOptions - Chart configuration options
 */
window.renderChart = function (chartId, chartType, chartData, chartOptions) {
    try {
        const ctx = document.getElementById(chartId);
        if (!ctx) {
            console.error(`Canvas element with ID '${chartId}' not found`);
            return;
        }

        // Destroy existing chart if it exists
        if (chartInstances[chartId]) {
            chartInstances[chartId].destroy();
        }

        // Get theme colors
        const isDark = document.documentElement.classList.contains('dark');
        const textColor = isDark ? '#f9fafb' : '#1f2937';
        const gridColor = isDark ? 'rgba(255, 255, 255, 0.1)' : 'rgba(0, 0, 0, 0.1)';

        // Default options
        const defaultOptions = {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    position: 'top',
                    labels: {
                        color: textColor,
                        padding: 15,
                        font: {
                            size: 12,
                            weight: '500'
                        },
                        usePointStyle: true,
                        pointStyle: 'circle'
                    }
                },
                tooltip: {
                    enabled: true,
                    backgroundColor: isDark ? 'rgba(31, 41, 55, 0.95)' : 'rgba(255, 255, 255, 0.95)',
                    titleColor: textColor,
                    bodyColor: textColor,
                    borderColor: isDark ? 'rgba(255, 255, 255, 0.2)' : 'rgba(0, 0, 0, 0.2)',
                    borderWidth: 1,
                    padding: 12,
                    cornerRadius: 8,
                    displayColors: true,
                    callbacks: {
                        label: function (context) {
                            let label = context.dataset.label || '';
                            if (label) {
                                label += ': ';
                            }
                            if (context.parsed.y !== null) {
                                label += context.parsed.y.toLocaleString();
                            }
                            return label;
                        }
                    }
                }
            },
            scales: {}
        };

        // Configure scales for charts that need them
        if (['line', 'bar'].includes(chartType)) {
            defaultOptions.scales = {
                y: {
                    beginAtZero: true,
                    grid: {
                        color: gridColor,
                        drawBorder: false
                    },
                    ticks: {
                        color: textColor,
                        font: {
                            size: 11
                        }
                    }
                },
                x: {
                    grid: {
                        color: gridColor,
                        drawBorder: false
                    },
                    ticks: {
                        color: textColor,
                        font: {
                            size: 11
                        }
                    }
                }
            };
        }

        // Merge custom options with defaults
        const finalOptions = {
            ...defaultOptions,
            ...chartOptions
        };

        // Create the chart
        chartInstances[chartId] = new Chart(ctx, {
            type: chartType,
            data: chartData,
            options: finalOptions
        });

        console.log(`Chart '${chartId}' rendered successfully`);
    } catch (error) {
        console.error(`Error rendering chart '${chartId}':`, error);
    }
};

/**
 * Update an existing chart with new data
 * @param {string} chartId - The ID of the canvas element
 * @param {object} newData - New chart data
 */
window.updateChart = function (chartId, newData) {
    try {
        if (chartInstances[chartId]) {
            chartInstances[chartId].data = newData;
            chartInstances[chartId].update();
            console.log(`Chart '${chartId}' updated successfully`);
        } else {
            console.warn(`Chart '${chartId}' not found`);
        }
    } catch (error) {
        console.error(`Error updating chart '${chartId}':`, error);
    }
};

/**
 * Destroy a chart instance
 * @param {string} chartId - The ID of the canvas element
 */
window.destroyChart = function (chartId) {
    try {
        if (chartInstances[chartId]) {
            chartInstances[chartId].destroy();
            delete chartInstances[chartId];
            console.log(`Chart '${chartId}' destroyed successfully`);
        }
    } catch (error) {
        console.error(`Error destroying chart '${chartId}':`, error);
    }
};

/**
 * Export a chart as PNG
 * @param {string} chartId - The ID of the canvas element
 * @param {string} fileName - The name of the file to download
 */
window.exportChartAsPNG = async function (chartId, fileName) {
    try {
        const canvas = document.getElementById(chartId);
        if (!canvas) {
            console.error(`Canvas element with ID '${chartId}' not found`);
            return;
        }

        const url = canvas.toDataURL('image/png');
        const link = document.createElement('a');
        link.download = fileName || 'chart.png';
        link.href = url;
        link.click();

        console.log(`Chart '${chartId}' exported as PNG`);
    } catch (error) {
        console.error(`Error exporting chart '${chartId}' as PNG:`, error);
    }
};

/**
 * Export report content as PDF
 * @param {string} elementId - The ID of the element to export
 * @param {string} fileName - The name of the PDF file
 * @param {string} orientation - Page orientation (portrait or landscape)
 * @param {boolean} includeCharts - Whether to include charts
 */
window.exportReportAsPDF = async function (elementId, fileName, orientation = 'portrait', includeCharts = true) {
    try {
        const element = document.getElementById(elementId);
        if (!element) {
            console.error(`Element with ID '${elementId}' not found`);
            return;
        }

        // Use html2canvas to capture the content
        const canvas = await html2canvas(element, {
            scale: 2,
            useCORS: true,
            logging: false,
            backgroundColor: document.documentElement.classList.contains('dark') ? '#111827' : '#ffffff'
        });

        const imgData = canvas.toDataURL('image/png');
        const pdf = new jspdf.jsPDF({
            orientation: orientation,
            unit: 'mm',
            format: 'a4'
        });

        const pageWidth = pdf.internal.pageSize.getWidth();
        const pageHeight = pdf.internal.pageSize.getHeight();
        const imgWidth = pageWidth - 20; // 10mm margin on each side
        const imgHeight = (canvas.height * imgWidth) / canvas.width;

        let heightLeft = imgHeight;
        let position = 10;

        // Add first page
        pdf.addImage(imgData, 'PNG', 10, position, imgWidth, imgHeight);
        heightLeft -= (pageHeight - 20);

        // Add additional pages if needed
        while (heightLeft > 0) {
            position = heightLeft - imgHeight + 10;
            pdf.addPage();
            pdf.addImage(imgData, 'PNG', 10, position, imgWidth, imgHeight);
            heightLeft -= (pageHeight - 20);
        }

        pdf.save(fileName || 'report.pdf');
        console.log('Report exported as PDF successfully');
    } catch (error) {
        console.error('Error exporting report as PDF:', error);
        alert('Failed to export PDF. Please try again.');
    }
};

/**
 * Export report content as Excel
 * @param {string} elementId - The ID of the element to export
 * @param {string} fileName - The name of the Excel file
 */
window.exportReportAsExcel = async function (elementId, fileName) {
    try {
        const element = document.getElementById(elementId);
        if (!element) {
            console.error(`Element with ID '${elementId}' not found`);
            return;
        }

        // Find all tables in the content
        const tables = element.querySelectorAll('table');
        if (tables.length === 0) {
            console.warn('No tables found in the content');
            return;
        }

        // Create a new workbook
        const wb = XLSX.utils.book_new();

        // Add each table as a separate sheet
        tables.forEach((table, index) => {
            const ws = XLSX.utils.table_to_sheet(table);
            const sheetName = `Sheet${index + 1}`;
            XLSX.utils.book_append_sheet(wb, ws, sheetName);
        });

        // If no tables, create a simple data sheet
        if (tables.length === 0) {
            const data = [['Report Data']];
            const ws = XLSX.utils.aoa_to_sheet(data);
            XLSX.utils.book_append_sheet(wb, ws, 'Report');
        }

        // Save the file
        XLSX.writeFile(wb, fileName || 'report.xlsx');
        console.log('Report exported as Excel successfully');
    } catch (error) {
        console.error('Error exporting report as Excel:', error);
        alert('Failed to export Excel. Please try again.');
    }
};

/**
 * Export report content as CSV
 * @param {string} elementId - The ID of the element to export
 * @param {string} fileName - The name of the CSV file
 */
window.exportReportAsCSV = async function (elementId, fileName) {
    try {
        const element = document.getElementById(elementId);
        if (!element) {
            console.error(`Element with ID '${elementId}' not found`);
            return;
        }

        // Find the first table in the content
        const table = element.querySelector('table');
        if (!table) {
            console.warn('No table found in the content');
            return;
        }

        // Convert table to CSV
        let csv = [];
        const rows = table.querySelectorAll('tr');

        rows.forEach(row => {
            const cols = row.querySelectorAll('td, th');
            const rowData = [];
            cols.forEach(col => {
                rowData.push('"' + col.innerText.replace(/"/g, '""') + '"');
            });
            csv.push(rowData.join(','));
        });

        const csvContent = csv.join('\n');
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        const url = URL.createObjectURL(blob);

        link.setAttribute('href', url);
        link.setAttribute('download', fileName || 'report.csv');
        link.style.visibility = 'hidden';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        console.log('Report exported as CSV successfully');
    } catch (error) {
        console.error('Error exporting report as CSV:', error);
        alert('Failed to export CSV. Please try again.');
    }
};

/**
 * Export element as PNG image
 * @param {string} elementId - The ID of the element to export
 * @param {string} fileName - The name of the PNG file
 */
window.exportReportAsPNG = async function (elementId, fileName) {
    try {
        const element = document.getElementById(elementId);
        if (!element) {
            console.error(`Element with ID '${elementId}' not found`);
            return;
        }

        const canvas = await html2canvas(element, {
            scale: 2,
            useCORS: true,
            logging: false,
            backgroundColor: document.documentElement.classList.contains('dark') ? '#111827' : '#ffffff'
        });

        canvas.toBlob(blob => {
            const link = document.createElement('a');
            link.download = fileName || 'report.png';
            link.href = URL.createObjectURL(blob);
            link.click();
        });

        console.log('Report exported as PNG successfully');
    } catch (error) {
        console.error('Error exporting report as PNG:', error);
        alert('Failed to export PNG. Please try again.');
    }
};

/**
 * Update all charts when theme changes
 */
window.updateChartsTheme = function () {
    Object.keys(chartInstances).forEach(chartId => {
        const chart = chartInstances[chartId];
        if (chart) {
            const isDark = document.documentElement.classList.contains('dark');
            const textColor = isDark ? '#f9fafb' : '#1f2937';
            const gridColor = isDark ? 'rgba(255, 255, 255, 0.1)' : 'rgba(0, 0, 0, 0.1)';

            // Update legend colors
            if (chart.options.plugins.legend) {
                chart.options.plugins.legend.labels.color = textColor;
            }

            // Update tooltip colors
            if (chart.options.plugins.tooltip) {
                chart.options.plugins.tooltip.backgroundColor = isDark ? 'rgba(31, 41, 55, 0.95)' : 'rgba(255, 255, 255, 0.95)';
                chart.options.plugins.tooltip.titleColor = textColor;
                chart.options.plugins.tooltip.bodyColor = textColor;
            }

            // Update scale colors
            if (chart.options.scales) {
                if (chart.options.scales.y) {
                    chart.options.scales.y.ticks.color = textColor;
                    chart.options.scales.y.grid.color = gridColor;
                }
                if (chart.options.scales.x) {
                    chart.options.scales.x.ticks.color = textColor;
                    chart.options.scales.x.grid.color = gridColor;
                }
            }

            chart.update();
        }
    });
};

// Listen for theme changes
if (typeof MutationObserver !== 'undefined') {
    const observer = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
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

console.log('Reports utilities loaded successfully');
