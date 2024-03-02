
describe('Тест Notes', () => {

    it('проверка сохранения записи', () => {
     
        cy.visit('https://localhost:7297/AnswerNotes?id=1')
        const text = Date()
        cy.get('#note_40').clear().type(text).blur();
        cy.get('#saveBtn_40').click();
        cy.get('#note_40').should('have.value', text);
    });

it('Add record test', () => {

        cy.visit('https://localhost:7297/AnswerNotes?id=51')
        const text = 'Date()'
        cy.get('#newNote').clear().type(text).blur();
        cy.get('#addBtn').click();
        let id= null;
      const items=  cy.get('#items')
          cy.log(`items: ${items}`);
        cy.get('#items').find('textarea').each(($textarea) => {
            // Получаем id каждого элемента textarea и выводим в консоль
            id = $textarea.attr('id');
            cy.log(`Textarea ID: ${id}`);
          });
        cy.get('#'+id).should('have.value', text);
    });
    

it('проверка Удаления записи', () => {

        cy.visit('https://localhost:7297/AnswerNotes?id=1')
        const text = 'Date()'
        cy.get('#note_40').clear().type(text).blur();
        cy.get('#saveBtn_40').click();
        cy.get('#note_40').should('have.value', text);
    });
    

})




