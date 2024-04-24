

describe('Тест журнала Custody Log', () => {

 it('Проверка наличия заголовка', () => {
     cy.visit('Common/CustodyLogJournal')
    cy.contains('H1', 'Custody Log')
    
  })  
  
  it('проверка фильра на наличие записей', () => {
      cy.visit('Common/CustodyLogJournal')

    cy.get('#d1From').type('1023-12-20').blur();
    cy.get('#d1To').type('2024-12-31').blur();
    cy.get('#findButton').click();
    cy.wait(1000);

    cy.get('.dx-info').invoke('text').then((text) => {
      // регулярное выражение для извлечения числа из текста Page 1 of 1 (0 items)
      const matches = text.match(/\((\d+)/);
      
     // Проверяем, что удалось извлечь число и оно больше 0
     const extractedNumber = matches && parseInt(matches[1], 10);
     cy.wrap(extractedNumber).should('be.gt', 0);
    })
   
  })

  it('Проверка фильтра на отсутствие записей', () => {
      cy.visit('Common/CustodyLogJournal')
    cy.get('#d1From').type('2025-12-30').blur();//unreal date tnere no records
    cy.get('#d1To').type('2023-12-30').blur();
    cy.get('#findButton').click();
    cy.wait(1000);
    cy.get('.dx-info').invoke('text').then((text) => {
      //регулярное выражение для извлечения числа из текста Page 1 of 1 (0 items)
      const matches = text.match(/\((-?\d+)/);
     // Проверяем, что удалось извлечь число и оно меньше равно 0
     const extractedNumber = matches && parseInt(matches[1], 10);
        cy.wrap(extractedNumber).should('be.lte', 0);
    })
  })
})