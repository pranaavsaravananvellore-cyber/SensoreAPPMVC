document.querySelectorAll("canvas.heatmap").forEach(canvas => {
    const ctx = canvas.getContext("2d");
    const grid = JSON.parse(canvas.dataset.grid);

    const size = 32;
    const cell = canvas.width / size;

    for (let i = 0; i < size; i++) {
        for (let j = 0; j < size; j++) {
            const v = grid[i][j];
            if (v <= 0) continue;

            const intensity = Math.min(255, Math.floor(v));
            ctx.fillStyle = `rgb(${intensity},0,0)`;

            ctx.fillRect(j * cell, i * cell, cell, cell);
        }
    }
});
