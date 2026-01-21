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

/**
 * Render a custom report chart for the report builder
 * @param {object} config - Chart configuration
 */
window.renderCustomReportChart = function (config) {
    try {
        const ctx = document.getElementById(config.chartId);
        if (!ctx) {
            console.error(`Canvas element with ID '${config.chartId}' not found`);
            return;
        }

        // Destroy existing chart if it exists
        if (chartInstances[config.chartId]) {
            chartInstances[config.chartId].destroy();
        }

        const isDark = document.documentElement.classList.contains('dark');
        const textColor = isDark ? '#f9fafb' : '#1f2937';
        const gridColor = isDark ? 'rgba(255, 255, 255, 0.1)' : 'rgba(0, 0, 0, 0.1)';

        // Generate colors for datasets
        const colors = [
            config.primaryColor || '#3b82f6',
            config.accentColor || '#8b5cf6',
            '#10b981',
            '#f59e0b',
            '#ef4444',
            '#ec4899',
            '#06b6d4',
            '#84cc16'
        ];

        // Build datasets
        const datasets = config.datasets.map((dataset, index) => {
            const color = colors[index % colors.length];
            const baseDataset = {
                label: dataset.label,
                data: dataset.data,
                backgroundColor: config.chartType === 'line' || config.chartType === 'radar' 
                    ? `${color}33` 
                    : config.chartType === 'pie' || config.chartType === 'doughnut'
                        ? dataset.data.map((_, i) => colors[i % colors.length])
                        : color,
                borderColor: config.chartType === 'pie' || config.chartType === 'doughnut'
                    ? dataset.data.map((_, i) => colors[i % colors.length])
                    : color,
                borderWidth: 2
            };

            // Add specific properties based on chart type
            if (config.chartType === 'line' || config.chartType === 'area') {
                baseDataset.tension = 0.4;
                baseDataset.fill = config.chartType === 'area';
                baseDataset.pointRadius = 4;
                baseDataset.pointHoverRadius = 6;
            }

            if (config.chartType === 'bar') {
                baseDataset.borderRadius = 6;
                baseDataset.maxBarThickness = 50;
            }

            return baseDataset;
        });

        // Determine actual chart type for Chart.js
        let actualChartType = config.chartType;
        if (config.chartType === 'area') {
            actualChartType = 'line';
        } else if (config.chartType === 'doughnut') {
            actualChartType = 'doughnut';
        } else if (config.chartType === 'table') {
            // Table is not a Chart.js type - render as bar chart for preview
            actualChartType = 'bar';
        }

        // Build options
        const options = {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: config.showLegend !== false,
                    position: 'top',
                    labels: {
                        color: textColor,
                        padding: 15,
                        font: { size: 12, weight: '500' },
                        usePointStyle: true
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
                    cornerRadius: 8
                },
                datalabels: config.showDataLabels ? {
                    display: true,
                    color: textColor,
                    font: { weight: 'bold', size: 11 },
                    formatter: (value) => value.toLocaleString()
                } : { display: false }
            }
        };

        // Add scales for appropriate chart types
        if (['bar', 'line', 'area'].includes(config.chartType)) {
            options.scales = {
                y: {
                    beginAtZero: true,
                    grid: { color: gridColor, drawBorder: false },
                    ticks: { color: textColor, font: { size: 11 } }
                },
                x: {
                    grid: { color: gridColor, drawBorder: false },
                    ticks: { color: textColor, font: { size: 11 } }
                }
            };
        }

        // Create the chart
        chartInstances[config.chartId] = new Chart(ctx, {
            type: actualChartType,
            data: {
                labels: config.labels,
                datasets: datasets
            },
            options: options
        });

        console.log(`Custom report chart '${config.chartId}' rendered successfully`);
    } catch (error) {
        console.error(`Error rendering custom report chart:`, error);
    }
};

/**
 * Download a text file
 * @param {string} filename - The name of the file
 * @param {string} content - The content to write
 */
window.downloadTextFile = function (filename, content) {
    try {
        const blob = new Blob([content], { type: 'application/json' });
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(link.href);
        console.log(`File '${filename}' downloaded successfully`);
    } catch (error) {
        console.error('Error downloading file:', error);
    }
};

/**
 * Generate a comprehensive report with combined and individual sections
 * @param {object} config - Report configuration with all sections
 */
window.generateComprehensiveReport = async function (config) {
    try {
        const { jsPDF } = window.jspdf;
        if (!jsPDF) {
            console.error('jsPDF library not loaded');
            return false;
        }

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

        // Colors
        const primaryColor = config.primaryColor || '#3b82f6';
        const accentColor = config.accentColor || '#8b5cf6';
        const colors = [primaryColor, accentColor, '#10b981', '#f59e0b', '#ef4444', '#ec4899', '#06b6d4', '#84cc16'];

        // Helper function to add new page
        const checkNewPage = (requiredHeight = 40) => {
            if (yPos + requiredHeight > pageHeight - margin) {
                pdf.addPage();
                yPos = margin;
                return true;
            }
            return false;
        };

        // Helper to convert hex to RGB
        const hexToRgb = (hex) => {
            const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
            return result ? {
                r: parseInt(result[1], 16),
                g: parseInt(result[2], 16),
                b: parseInt(result[3], 16)
            } : { r: 59, g: 130, b: 246 };
        };

        // ========== PAGE 1: COVER PAGE ==========
        // Header gradient background
        const headerRgb = hexToRgb(primaryColor);
        pdf.setFillColor(headerRgb.r, headerRgb.g, headerRgb.b);
        pdf.rect(0, 0, pageWidth, 80, 'F');

        // Title
        pdf.setTextColor(255, 255, 255);
        pdf.setFontSize(28);
        pdf.setFont('helvetica', 'bold');
        pdf.text(config.title || 'Custom Report', margin, 35);

        // Subtitle
        pdf.setFontSize(12);
        pdf.setFont('helvetica', 'normal');
        pdf.text(config.subtitle || '', margin, 48);

        // Description
        if (config.description) {
            pdf.setFontSize(10);
            pdf.text(config.description.substring(0, 100), margin, 60);
        }

        // Logo/Brand
        pdf.setFontSize(14);
        pdf.setFont('helvetica', 'bold');
        pdf.text('MediChat.AI', pageWidth - margin - 30, 35);
        pdf.setFontSize(9);
        pdf.setFont('helvetica', 'normal');
        pdf.text(config.generatedAt || new Date().toLocaleDateString(), pageWidth - margin - 30, 45);

        yPos = 95;
        pdf.setTextColor(0, 0, 0);

        // ========== EXECUTIVE SUMMARY ==========
        pdf.setFontSize(16);
        pdf.setFont('helvetica', 'bold');
        pdf.text('Executive Summary', margin, yPos);
        yPos += 12;

        // Summary cards
        const overview = config.combinedOverview || {};
        const summaryItems = [
            { label: 'Total Metrics', value: overview.totalMetrics || 0 },
            { label: 'Data Sources', value: overview.totalDataSources || 0 },
            { label: 'Date Range', value: overview.dateRange || 'N/A' },
            { label: 'Category', value: config.category || 'Custom' }
        ];

        const cardWidth = (contentWidth - 15) / 4;
        summaryItems.forEach((item, index) => {
            const x = margin + (index * (cardWidth + 5));
            
            // Card background
            pdf.setFillColor(245, 247, 250);
            pdf.roundedRect(x, yPos, cardWidth, 25, 3, 3, 'F');
            
            // Label
            pdf.setFontSize(8);
            pdf.setTextColor(100, 100, 100);
            pdf.setFont('helvetica', 'normal');
            pdf.text(item.label, x + 5, yPos + 8);
            
            // Value
            pdf.setFontSize(12);
            pdf.setTextColor(0, 0, 0);
            pdf.setFont('helvetica', 'bold');
            const valueText = String(item.value).substring(0, 15);
            pdf.text(valueText, x + 5, yPos + 18);
        });
        yPos += 35;

        // ========== DATA SOURCES SECTION ==========
        if (config.dataSources && config.dataSources.length > 0) {
            checkNewPage(50);
            
            pdf.setFontSize(14);
            pdf.setFont('helvetica', 'bold');
            pdf.setTextColor(0, 0, 0);
            pdf.text('Data Sources Included', margin, yPos);
            yPos += 10;

            config.dataSources.forEach((ds, index) => {
                checkNewPage(25);
                
                const colorRgb = hexToRgb(colors[index % colors.length]);
                
                // Light background for the card (using light gray)
                pdf.setFillColor(248, 250, 252);
                pdf.roundedRect(margin, yPos, contentWidth, 18, 2, 2, 'F');
                
                // Add subtle border
                pdf.setDrawColor(226, 232, 240);
                pdf.setLineWidth(0.5);
                pdf.roundedRect(margin, yPos, contentWidth, 18, 2, 2, 'S');
                
                // Color indicator bar on the left
                pdf.setFillColor(colorRgb.r, colorRgb.g, colorRgb.b);
                pdf.roundedRect(margin, yPos, 4, 18, 2, 0, 'F');
                
                pdf.setFontSize(11);
                pdf.setFont('helvetica', 'bold');
                pdf.setTextColor(30, 41, 59);
                pdf.text(ds.name || 'Unknown', margin + 10, yPos + 8);
                
                pdf.setFontSize(9);
                pdf.setFont('helvetica', 'normal');
                pdf.setTextColor(100, 116, 139);
                pdf.text(ds.description || '', margin + 10, yPos + 14);
                
                // Records count on the right side
                pdf.setFontSize(9);
                pdf.setTextColor(71, 85, 105);
                const recordsText = `${ds.recordsInPeriod || 0} of ${ds.recordCount || 0} records`;
                pdf.text(recordsText, pageWidth - margin - 50, yPos + 11);
                
                yPos += 22;
            });
            yPos += 10;
        }

        // ========== COMBINED CHART SECTION ==========
        checkNewPage(100);
        
        pdf.setFontSize(14);
        pdf.setFont('helvetica', 'bold');
        pdf.setTextColor(0, 0, 0);
        pdf.text('Combined Overview Chart', margin, yPos);
        yPos += 10;

        // Create combined chart canvas
        if (config.combinedChart && config.combinedChart.datasets) {
            // For table type, skip chart and render data table instead
            if (config.combinedChart.chartType === 'table') {
                // Render data table instead of chart
                pdf.setFontSize(10);
                pdf.setFont('helvetica', 'bold');
                pdf.text('Data Table', margin, yPos);
                yPos += 8;

                // Table header
                pdf.setFillColor(240, 240, 240);
                pdf.rect(margin, yPos, contentWidth, 10, 'F');
                pdf.setFontSize(9);
                pdf.setTextColor(60, 60, 60);
                pdf.text('Label', margin + 5, yPos + 7);
                
                const datasets = config.combinedChart.datasets || [];
                const colWidth = (contentWidth - 60) / Math.max(datasets.length, 1);
                datasets.forEach((ds, idx) => {
                    pdf.text(ds.label || `Metric ${idx + 1}`, margin + 60 + (idx * colWidth), yPos + 7);
                });
                yPos += 12;

                // Table rows
                const labels = config.combinedChart.labels || [];
                labels.slice(0, 15).forEach((label, rowIdx) => {
                    if (yPos > pageHeight - 30) return;
                    
                    pdf.setFillColor(rowIdx % 2 === 0 ? 255 : 248, rowIdx % 2 === 0 ? 255 : 250, rowIdx % 2 === 0 ? 255 : 252);
                    pdf.rect(margin, yPos, contentWidth, 8, 'F');
                    
                    pdf.setFontSize(8);
                    pdf.setTextColor(0, 0, 0);
                    pdf.setFont('helvetica', 'normal');
                    pdf.text(String(label).substring(0, 20), margin + 5, yPos + 6);
                    
                    datasets.forEach((ds, idx) => {
                        const value = ds.data && ds.data[rowIdx] !== undefined ? ds.data[rowIdx] : 0;
                        pdf.text(typeof value === 'number' ? value.toLocaleString() : String(value), margin + 60 + (idx * colWidth), yPos + 6);
                    });
                    yPos += 8;
                });
                yPos += 10;
            } else {
                try {
                    const chartCanvas = document.createElement('canvas');
                    chartCanvas.width = 800;
                    chartCanvas.height = 400;
                    document.body.appendChild(chartCanvas);

                    const chartType = config.combinedChart.chartType === 'area' ? 'line' : config.combinedChart.chartType;
                    
                    const chartData = {
                        labels: config.combinedChart.labels || [],
                        datasets: config.combinedChart.datasets.map((ds, i) => ({
                            label: ds.label,
                            data: ds.data,
                            backgroundColor: chartType === 'pie' || chartType === 'doughnut' 
                                ? colors.slice(0, ds.data.length)
                                : colors[i % colors.length] + '80',
                            borderColor: colors[i % colors.length],
                            borderWidth: 2,
                            tension: 0.4,
                            fill: config.combinedChart.chartType === 'area'
                        }))
                    };

                    const chart = new Chart(chartCanvas, {
                        type: chartType,
                        data: chartData,
                        options: {
                            responsive: false,
                            animation: false,
                            plugins: {
                                legend: { display: config.showLegend !== false }
                            },
                            scales: ['bar', 'line'].includes(chartType) ? {
                                y: { beginAtZero: true }
                            } : undefined
                        }
                    });

                    await new Promise(resolve => setTimeout(resolve, 500));
                    
                    const chartImg = chartCanvas.toDataURL('image/png');
                    pdf.addImage(chartImg, 'PNG', margin, yPos, contentWidth, 70);
                    yPos += 80;

                    chart.destroy();
                    document.body.removeChild(chartCanvas);
                } catch (err) {
                    console.error('Error generating combined chart:', err);
                    yPos += 10;
                }
            }
        }

        // ========== INDIVIDUAL METRIC SECTIONS ==========
        if (config.metricSections && config.metricSections.length > 0) {
            pdf.addPage();
            yPos = margin;

            pdf.setFontSize(18);
            pdf.setFont('helvetica', 'bold');
            pdf.setTextColor(headerRgb.r, headerRgb.g, headerRgb.b);
            pdf.text('Individual Metric Analysis', margin, yPos);
            yPos += 15;

            for (let i = 0; i < config.metricSections.length; i++) {
                const metric = config.metricSections[i];
                checkNewPage(120);

                const colorRgb = hexToRgb(colors[i % colors.length]);

                // Metric header
                pdf.setFillColor(colorRgb.r, colorRgb.g, colorRgb.b);
                pdf.rect(margin, yPos, contentWidth, 12, 'F');
                
                pdf.setFontSize(12);
                pdf.setFont('helvetica', 'bold');
                pdf.setTextColor(255, 255, 255);
                pdf.text(`${i + 1}. ${metric.name}`, margin + 5, yPos + 8);
                
                pdf.setFontSize(9);
                pdf.text(`Aggregation: ${metric.aggregation || 'Sum'} | Type: ${metric.dataType || 'Number'}`, pageWidth - margin - 60, yPos + 8);
                yPos += 18;

                // Metric summary stats
                if (metric.summary) {
                    const stats = [
                        { label: 'Total', value: metric.summary.total },
                        { label: 'Average', value: metric.summary.average },
                        { label: 'Maximum', value: metric.summary.max },
                        { label: 'Minimum', value: metric.summary.min }
                    ];

                    const statWidth = (contentWidth - 15) / 4;
                    stats.forEach((stat, idx) => {
                        const x = margin + (idx * (statWidth + 5));
                        pdf.setFillColor(250, 250, 250);
                        pdf.roundedRect(x, yPos, statWidth, 20, 2, 2, 'F');
                        
                        pdf.setFontSize(8);
                        pdf.setTextColor(100, 100, 100);
                        pdf.setFont('helvetica', 'normal');
                        pdf.text(stat.label, x + 3, yPos + 7);
                        
                        pdf.setFontSize(11);
                        pdf.setTextColor(0, 0, 0);
                        pdf.setFont('helvetica', 'bold');
                        pdf.text(String(stat.value), x + 3, yPos + 16);
                    });
                    yPos += 28;
                }

                // Metric chart
                try {
                    const metricCanvas = document.createElement('canvas');
                    metricCanvas.width = 600;
                    metricCanvas.height = 250;
                    document.body.appendChild(metricCanvas);

                    // Convert table/area to valid Chart.js types
                    let metricChartType = metric.chartType || 'bar';
                    if (metricChartType === 'area') metricChartType = 'line';
                    if (metricChartType === 'table') metricChartType = 'bar';
                    
                    const metricChart = new Chart(metricCanvas, {
                        type: metricChartType,
                        data: {
                            labels: metric.labels || [],
                            datasets: [{
                                label: metric.name,
                                data: metric.values || [],
                                backgroundColor: metricChartType === 'pie' || metricChartType === 'doughnut'
                                    ? colors.slice(0, (metric.values || []).length)
                                    : colors[i % colors.length] + '80',
                                borderColor: colors[i % colors.length],
                                borderWidth: 2,
                                tension: 0.4,
                                fill: metric.chartType === 'area'
                            }]
                        },
                        options: {
                            responsive: false,
                            animation: false,
                            plugins: { legend: { display: false } },
                            scales: ['bar', 'line'].includes(metricChartType) ? {
                                y: { beginAtZero: true }
                            } : undefined
                        }
                    });

                    await new Promise(resolve => setTimeout(resolve, 300));
                    
                    const metricImg = metricCanvas.toDataURL('image/png');
                    pdf.addImage(metricImg, 'PNG', margin, yPos, contentWidth, 50);
                    yPos += 58;

                    metricChart.destroy();
                    document.body.removeChild(metricCanvas);
                } catch (err) {
                    console.error(`Error generating chart for ${metric.name}:`, err);
                    yPos += 10;
                }

                // Data table for this metric
                if (config.includeTable && metric.labels && metric.values) {
                    checkNewPage(50);
                    
                    pdf.setFontSize(10);
                    pdf.setFont('helvetica', 'bold');
                    pdf.setTextColor(0, 0, 0);
                    pdf.text('Data Table', margin, yPos);
                    yPos += 6;

                    // Table header
                    pdf.setFillColor(240, 240, 240);
                    pdf.rect(margin, yPos, contentWidth, 8, 'F');
                    pdf.setFontSize(8);
                    pdf.text('Category', margin + 3, yPos + 5);
                    pdf.text('Value', margin + contentWidth/2, yPos + 5);
                    yPos += 8;

                    // Table rows (max 8)
                    const maxRows = Math.min(8, metric.labels.length);
                    for (let j = 0; j < maxRows; j++) {
                        if (j % 2 === 0) {
                            pdf.setFillColor(250, 250, 250);
                            pdf.rect(margin, yPos, contentWidth, 6, 'F');
                        }
                        pdf.setFont('helvetica', 'normal');
                        pdf.setFontSize(8);
                        pdf.text(String(metric.labels[j]).substring(0, 30), margin + 3, yPos + 4);
                        pdf.text(String(metric.values[j]).toLocaleString(), margin + contentWidth/2, yPos + 4);
                        yPos += 6;
                    }
                    yPos += 10;
                }

                yPos += 5;
            }
        }

        // ========== FILTERS APPLIED ==========
        if (config.filtersApplied && config.filtersApplied.length > 0) {
            checkNewPage(40);
            
            pdf.setFontSize(12);
            pdf.setFont('helvetica', 'bold');
            pdf.setTextColor(0, 0, 0);
            pdf.text('Filters Applied', margin, yPos);
            yPos += 8;

            config.filtersApplied.forEach(filter => {
                pdf.setFontSize(9);
                pdf.setFont('helvetica', 'normal');
                pdf.text(`• ${filter.field} ${filter.operator} "${filter.value}"`, margin + 5, yPos);
                yPos += 6;
            });
            yPos += 10;
        }

        // ========== FOOTER ==========
        const totalPages = pdf.internal.getNumberOfPages();
        for (let p = 1; p <= totalPages; p++) {
            pdf.setPage(p);
            
            // Footer line
            pdf.setDrawColor(200, 200, 200);
            pdf.line(margin, pageHeight - 15, pageWidth - margin, pageHeight - 15);
            
            // Footer text
            pdf.setFontSize(8);
            pdf.setTextColor(128, 128, 128);
            pdf.setFont('helvetica', 'normal');
            pdf.text('Generated by MediChat.AI - Healthcare Communication Platform', margin, pageHeight - 8);
            pdf.text(`Page ${p} of ${totalPages}`, pageWidth - margin - 20, pageHeight - 8);
        }

        // Save the PDF
        pdf.save(config.filename || 'comprehensive_report.pdf');
        console.log('Comprehensive report generated successfully');
        return true;

    } catch (error) {
        console.error('Error generating comprehensive report:', error);
        return false;
    }
};

/**
 * Generate an Excel report
 * @param {object} config - Report configuration
 */
window.generateExcelReport = async function (config) {
    try {
        // Create workbook data
        const workbookData = [];
        
        // Add header sheet info
        workbookData.push(['Report:', config.title || 'Custom Report']);
        workbookData.push(['Generated:', config.generatedAt || new Date().toLocaleDateString()]);
        workbookData.push(['Period:', config.subtitle || '']);
        workbookData.push(['Category:', config.category || 'Custom']);
        workbookData.push([]);
        
        // Add summary section
        if (config.combinedOverview) {
            workbookData.push(['=== SUMMARY ===']);
            workbookData.push(['Total Metrics:', config.combinedOverview.totalMetrics || 0]);
            workbookData.push(['Data Sources:', config.combinedOverview.totalDataSources || 0]);
            workbookData.push(['Date Range:', config.combinedOverview.dateRange || 'N/A']);
            workbookData.push([]);
        }
        
        // Add data sources
        if (config.dataSources && config.dataSources.length > 0) {
            workbookData.push(['=== DATA SOURCES ===']);
            workbookData.push(['Name', 'Description', 'Records in Period', 'Total Records']);
            config.dataSources.forEach(ds => {
                workbookData.push([ds.name, ds.description, ds.recordsInPeriod, ds.recordCount]);
            });
            workbookData.push([]);
        }
        
        // Add metric sections with data
        if (config.metricSections && config.metricSections.length > 0) {
            config.metricSections.forEach(metric => {
                workbookData.push([`=== ${metric.name.toUpperCase()} ===`]);
                workbookData.push(['Aggregation:', metric.aggregation, 'Type:', metric.dataType]);
                workbookData.push(['Total:', metric.summary?.total, 'Average:', metric.summary?.average]);
                workbookData.push(['Max:', metric.summary?.max, 'Min:', metric.summary?.min]);
                workbookData.push([]);
                
                // Add data table
                workbookData.push(['Label', 'Value']);
                if (metric.labels && metric.values) {
                    for (let i = 0; i < metric.labels.length; i++) {
                        workbookData.push([metric.labels[i], metric.values[i]]);
                    }
                }
                workbookData.push([]);
            });
        }
        
        // Add filters
        if (config.filtersApplied && config.filtersApplied.length > 0) {
            workbookData.push(['=== FILTERS APPLIED ===']);
            workbookData.push(['Field', 'Operator', 'Value']);
            config.filtersApplied.forEach(filter => {
                workbookData.push([filter.field, filter.operator, filter.value]);
            });
        }
        
        // Convert to CSV format for Excel compatibility
        let csvContent = workbookData.map(row => 
            row.map(cell => {
                const cellStr = String(cell ?? '');
                // Escape quotes and wrap in quotes if contains comma
                if (cellStr.includes(',') || cellStr.includes('"') || cellStr.includes('\n')) {
                    return `"${cellStr.replace(/"/g, '""')}"`;
                }
                return cellStr;
            }).join(',')
        ).join('\n');
        
        // Add BOM for Excel UTF-8 compatibility
        const BOM = '\uFEFF';
        csvContent = BOM + csvContent;
        
        // Create and download file as CSV (Excel can open CSV files directly)
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8' });
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        // Use .csv extension - Excel opens these properly and they're more reliable than fake .xlsx
        link.download = (config.filename || 'report').replace('.pdf', '').replace('.xlsx', '') + '.csv';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(link.href);
        
        console.log('Excel-compatible CSV report generated successfully');
        return true;
    } catch (error) {
        console.error('Error generating Excel report:', error);
        return false;
    }
};

/**
 * Generate a CSV report
 * @param {object} config - Report configuration
 */
window.generateCsvReport = async function (config) {
    try {
        const csvRows = [];
        
        // Add header
        csvRows.push(['Report', config.title || 'Custom Report']);
        csvRows.push(['Generated', config.generatedAt || new Date().toLocaleDateString()]);
        csvRows.push(['Period', config.subtitle || '']);
        csvRows.push([]);
        
        // Add combined data if available
        if (config.combinedChart && config.combinedChart.labels) {
            // Create header row with all metric names
            const headerRow = ['Label'];
            if (config.combinedChart.datasets) {
                config.combinedChart.datasets.forEach(ds => {
                    headerRow.push(ds.label);
                });
            }
            csvRows.push(headerRow);
            
            // Add data rows
            for (let i = 0; i < config.combinedChart.labels.length; i++) {
                const row = [config.combinedChart.labels[i]];
                if (config.combinedChart.datasets) {
                    config.combinedChart.datasets.forEach(ds => {
                        row.push(ds.data[i] ?? '');
                    });
                }
                csvRows.push(row);
            }
            csvRows.push([]);
        }
        
        // Add individual metric summaries
        if (config.metricSections && config.metricSections.length > 0) {
            csvRows.push(['Metric Summaries']);
            csvRows.push(['Metric', 'Total', 'Average', 'Max', 'Min']);
            config.metricSections.forEach(metric => {
                csvRows.push([
                    metric.name,
                    metric.summary?.total || 0,
                    metric.summary?.average || 0,
                    metric.summary?.max || 0,
                    metric.summary?.min || 0
                ]);
            });
        }
        
        // Convert to CSV
        let csvContent = csvRows.map(row => 
            row.map(cell => {
                const cellStr = String(cell ?? '');
                if (cellStr.includes(',') || cellStr.includes('"') || cellStr.includes('\n')) {
                    return `"${cellStr.replace(/"/g, '""')}"`;
                }
                return cellStr;
            }).join(',')
        ).join('\n');
        
        // Create and download
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8' });
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = (config.filename || 'report').replace('.pdf', '') + '.csv';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(link.href);
        
        console.log('CSV report generated successfully');
        return true;
    } catch (error) {
        console.error('Error generating CSV report:', error);
        return false;
    }
};

console.log('Reports utilities loaded successfully');
