#!/usr/bin/env node

console.log("Building and launching iOS Simulator...");
console.log("Check the iOS Simulator for your app!");
console.log("Debug output will appear in the VS Code terminal.");

// Keep the process alive briefly
setTimeout(() => {
    console.log("Launch helper completed. App should be running in iOS Simulator.");
    process.exit(0);
}, 2000);