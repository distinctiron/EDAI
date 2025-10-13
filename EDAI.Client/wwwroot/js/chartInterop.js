window.chartJsInterop = (() => {
    const chartRegistry = new Map();

    function destroyChart(id) {
        const existing = chartRegistry.get(id);
        if (existing) {
            existing.destroy();
            chartRegistry.delete(id);
        }
    }

    function createChart(canvasId, config) {
        const element = document.getElementById(canvasId);
        if (!element) {
            console.warn(`[chartJsInterop] Unable to find canvas with id '${canvasId}'.`);
            return;
        }

        if (typeof Chart === "undefined") {
            console.error("Chart.js is not loaded. Ensure the script is referenced before the Blazor application.");
            return;
        }

        destroyChart(canvasId);

        const chartInstance = new Chart(element, config);
        chartRegistry.set(canvasId, chartInstance);
    }

    return {
        createChart,
        destroyChart
    };
})();
