
const { defineConfig } = require('cypress')

module.exports = defineConfig({
    fixturesFolder: false,
    viewportHeight: 1000,
    viewportWidth: 1000,
    "types": ["cypress", "node", "cypress-ntlm-auth"],
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

