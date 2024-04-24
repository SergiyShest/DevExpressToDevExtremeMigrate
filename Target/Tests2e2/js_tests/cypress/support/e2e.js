// ***********************************************************
// This example support/e2e.js is processed and
// loaded automatically before your test files.
//
// This is a great place to put global configuration and
// behavior that modifies Cypress.
//
// You can change the location of this file or turn off
// automatically serving support files with the
// 'supportFile' configuration option.
//
// You can read more here:
// https://on.cypress.io/configuration
// ***********************************************************


// Import commands.js using ES2015 syntax:
import './commands'
//import "cypress-ntlm-auth/dist/commands";
// Alternatively you can use CommonJS syntax:
// require('./commands')

beforeEach(() => {
    //cy.visit('Account/Login/Index')
    //cy.get('#username').type("Shest_akow@rambler.ru").blur();
    //cy.get('#password').type("Adm").blur();
    //cy.get('#logonBtn').click();
    cy.log('I run before every test in every spec file!!!!!!')
    cy.request({
        method: 'POST',
        url: 'https://localhost:7210/Account/Login/Login', // baseUrl will be prepended to this url
        form: true, // indicates the body should be form urlencoded and sets Content-Type: application/x-www-form-urlencoded headers
        body: {
            username:"test",
            password:"test",
        },
    })
});