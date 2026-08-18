// JS interop module for the Chart.js dashboard variant (Components/DashboardChartJs, Pages/DashboardChartJs.razor).
// Loaded on demand via IJSRuntime dynamic import, so it never touches the MudBlazor-chart dashboard.
// Chart.js itself is lazy-loaded from ./vendor on first use, keeping the app's other pages free of it.

let chartJsLoadPromise = null;

function ensureChartJsLoaded() {
    if (window.Chart) {
        return Promise.resolve();
    }

    chartJsLoadPromise ??= new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = '/js/dashboard-chartjs/vendor/chart.umd.min.js';
        script.onload = () => resolve();
        script.onerror = () => reject(new Error('Failed to load Chart.js from vendor bundle.'));
        document.head.appendChild(script);
    });

    return chartJsLoadPromise;
}

const chartInstances = new Map();

export function destroyChart(canvasId) {
    const existing = chartInstances.get(canvasId);
    if (existing) {
        existing.destroy();
        chartInstances.delete(canvasId);
    }
}

export async function createMonthlyBarChart(canvasId, dotNetRef, labels, currentValues, previousValues, currentColor, previousColor) {
    await ensureChartJsLoaded();
    destroyChart(canvasId);

    const canvas = document.getElementById(canvasId);
    if (!canvas) {
        return;
    }

    const chart = new Chart(canvas, {
        type: 'bar',
        data: {
            labels,
            datasets: [
                { label: 'Current Year', data: currentValues, backgroundColor: currentColor, borderColor: '#212121', borderWidth: 0 },
                { label: 'Previous Year', data: previousValues, backgroundColor: previousColor, borderColor: '#212121', borderWidth: 0 }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: { beginAtZero: true, ticks: { precision: 0 } }
            },
            plugins: {
                legend: { display: false }
            },
            onClick(_event, elements) {
                if (!elements.length) {
                    return;
                }
                const { datasetIndex, index } = elements[0];
                dotNetRef.invokeMethodAsync('OnBarClicked', datasetIndex, index);
            },
            onHover(event, elements) {
                event.native.target.style.cursor = elements.length ? 'pointer' : 'default';
            }
        }
    });

    chartInstances.set(canvasId, chart);
}

export function highlightSelectedBar(canvasId, datasetIndex, index) {
    const chart = chartInstances.get(canvasId);
    if (!chart) {
        return;
    }

    chart.data.datasets.forEach((dataset, currentDatasetIndex) => {
        dataset.borderWidth = dataset.data.map((_, currentIndex) =>
            currentDatasetIndex === datasetIndex && currentIndex === index ? 3 : 0);
    });

    chart.update();
}

export async function createTypeBreakdownChart(canvasId, dotNetRef, labels, typeSeries) {
    await ensureChartJsLoaded();
    destroyChart(canvasId);

    const canvas = document.getElementById(canvasId);
    if (!canvas) {
        return;
    }

    const datasets = typeSeries.map(series => ({
        label: series.name,
        data: series.values,
        backgroundColor: series.color,
        borderColor: '#212121',
        borderWidth: 0
    }));

    const chart = new Chart(canvas, {
        type: 'bar',
        data: { labels, datasets },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                x: { stacked: true },
                y: { stacked: true, beginAtZero: true, ticks: { precision: 0 } }
            },
            plugins: {
                legend: { display: false }
            },
            onClick(_event, elements) {
                if (!elements.length) {
                    return;
                }
                dotNetRef.invokeMethodAsync('OnTypeBarClicked', elements[0].index);
            },
            onHover(event, elements) {
                event.native.target.style.cursor = elements.length ? 'pointer' : 'default';
            }
        }
    });

    chartInstances.set(canvasId, chart);
}

export function highlightSelectedTypeBar(canvasId, index) {
    const chart = chartInstances.get(canvasId);
    if (!chart) {
        return;
    }

    chart.data.datasets.forEach(dataset => {
        dataset.borderWidth = dataset.data.map((_, currentIndex) => (currentIndex === index ? 3 : 0));
    });

    chart.update();
}

export async function createNestedDonutChart(canvasId, rings) {
    await ensureChartJsLoaded();
    destroyChart(canvasId);

    const canvas = document.getElementById(canvasId);
    if (!canvas) {
        return;
    }

    const datasets = rings.map(ring => ({
        label: ring.name,
        data: ring.values,
        backgroundColor: ring.colors,
        borderColor: '#ffffff',
        borderWidth: 1
    }));

    const chart = new Chart(canvas, {
        type: 'doughnut',
        data: { labels: rings[0]?.labels ?? [], datasets },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '30%',
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label(context) {
                            const ring = rings[context.datasetIndex];
                            return `${ring.name} - ${ring.labels[context.dataIndex]}: ${ring.values[context.dataIndex]}`;
                        }
                    }
                }
            }
        }
    });

    chartInstances.set(canvasId, chart);
}
