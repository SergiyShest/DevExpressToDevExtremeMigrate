
const { defineConfig } = require('cypress')

module.exports = defineConfig({
    fixturesFolder: false,
    viewportHeight: 200,
    viewportWidth: 200,
    e2e: {
        baseUrl: 'https://localhost:7210',
        supportFile: false,
        experimentalStudio: true,
        video: true,
        supportFile: 'cypress/support/e2e.js',
        supportFolder: 'cypress/support',
        videoUploadOnPasses: true,
    },
            setupNodeEvents(on, config) {
              // implement node event listeners here
            },




});

