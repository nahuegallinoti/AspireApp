// Minimal TypeScript entry point — compiled by Microsoft.TypeScript.MSBuild
// into wwwroot/js/app.js. Use this file as the bootstrap for client-side
// behavior that augments the Blazor Server runtime (e.g. JS interop helpers,
// browser feature detection, third-party widget initialization).
export function ready(callback) {
    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", callback, { once: true });
    }
    else {
        callback();
    }
}
ready(() => {
    // Hook for global client init. Intentionally left empty.
});
//# sourceMappingURL=app.js.map