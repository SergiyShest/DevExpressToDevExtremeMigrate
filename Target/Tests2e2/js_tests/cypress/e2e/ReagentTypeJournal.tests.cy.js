describe('Тест журнала DoctorJournal', () => {

 it('Проверка наличия заголовка', () => {
     cy.visit('https://localhost:7210/Reagents/ReagentTypeJournal')
    cy.contains('H1', '')
    
  })  
  
  it('проверка фильра на наличие записей', () => {
      cy.visit('https://localhost:7210/Reagents/ReagentTypeJournal')

    cy.get('#d1From').type('1023-12-20').blur();
    cy.get('#d1To').type('2024-12-31').blur();
    cy.get('#findButton').click();
    cy.wait(2000);

    cy.get('.dx-info').invoke('text').then((text) => {
      // регулярное выражение для извлечения числа из текста Page 1 of 1 (0 items)
      const matches = text.match(/\((\d+)/);
      
     // Проверяем, что удалось извлечь число и оно больше 0
     const extractedNumber = matches && parseInt(matches[1], 10);
     cy.wrap(extractedNumber).should('be.gt', 0);
    })
   
  })

  it('Проверка фильтра на отсутствие записей', () => {
      cy.visit('https://localhost:7210/Reagents/ReagentTypeJournal')
    cy.get('#d1From').type('2000-12-31').blur();//unreal date tnere no records
    cy.get('#d1To').type('2000-12-31').blur();
    cy.get('#findButton').click();
    cy.wait(2000);
    cy.get('.dx-info').invoke('text').then((text) => {
      //регулярное выражение для извлечения числа из текста Page 1 of 1 (0 items)
      const matches = text.match(/\((\d+)/);
      
     // Проверяем, что удалось извлечь число и оно 0
     const extractedNumber = matches && parseInt(matches[1], 10);
     cy.wrap(extractedNumber).should('eq', 0);
    })
  })

 

})