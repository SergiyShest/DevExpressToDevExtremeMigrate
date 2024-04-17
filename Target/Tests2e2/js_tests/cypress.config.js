
const { defineConfig } = require('cypress')

module.exports = defineConfig({
    fixturesFolder: false,
    viewportHeight: 200,
    viewportWidth: 200,
    e2e: {
        baseUrl: 'https://localhost:7210',
        supportFile: false,
        //    setupNodeEvents(on, config) {
        //      // implement node event listeners here
        //    },
    },
})


