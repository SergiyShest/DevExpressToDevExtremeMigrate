
Пример демонстрирует автоматическую конвертацию Devexpress в Devextreme (необходимость возникла в рамках импортозамещения).
Не предлагаю ни чего нового, просто известный подход, приложенный к конкретной ситуации. 

Задача: Нужно много (в нашем случае более 70) однотипных классов/файлов, отличающихся заранее известными частями, которые по каким-то причинам не получится сделать Generic. 
 
Решение: Создавать по шаблону. С возможностью если необходимо потом вносить изменения. 
 
В студии есть встроенный кодогенератор, он реализует похожую задачу. Я изначально рассматривал использование его. Но передумал. Получается много сложнее.  Кроме того, генератор в студии предполагается использовать постоянно, мой генератор нужен одноразово, что бы создать заготовку кода. Хотя... 

 Репозиторий содержит Папки:

 1) Source: Файлы исходного проекта (приложение пример, содержит таблицы Devexpress), сознательно удалил из него все лишнее и оставил только необходимые файлы.
 2) Target: Работающий целевой проект.
 3) codegenerator: Генератор.
    
Особенности исходнного проекта:Написан по технлогии MVC. Большая часть это табличные формы и относительно простые карточки к ним. 

Особенности целевого проекта:
То же  MVC. В целях упрощения отказались от идеи использовать SPA. Каждая странница должна загружаться отдельно. Так же в целях упрощения было принято решение отказаться от сборщиков и загружать js библиотеки через cdn.
Генератор создает таблицы и карточки аналогичные таковым в исходном проекте по шаблонам . Текст шаблона в примере несколько упрощен. Но общей сути это не поменяло.
 
Для ускорения разработки и гибкости генератор управляется запуском тестов. Это удобно для программ, которые выполняются только разработчиком. 
 Тесты для управления вынесены в отдельный класс GeneratorCommand:
1)	CollectInfo(): Изначальный сбор информации в файл
2)	GenerateAll(): Генерация кода приложения и тестов
3)	GeneratePart(): Генерация отдельного контроллера

Предполагаемый порядок работы:
1)	Сбор данных: запуск теста GeneratorCommand.CollectInfo() логика работы которого приблизительно следующая: 
a)	 В исходном проекте UI ищутся все файлы по шаблону _*grid*.cshtml. В этих файлах содержится описание колонок таблицы и обычно есть модель, по которой можно понять какой класс Entity биндится с вьюхой или таблицей в базе данных. (иногда эту информацию приходится вытаскивать из контроллера).
b)	Ищу класс Entity на для того что бы взять описания полей для формы редактирования.
c)	Сохраняю часть собранной информации в файл (collector.json).
2)	Редактирование и дополнение собранной информации выполняется в ручном режиме
a)	Так как из-за разнородности исходного кода не для всех значений можно получить правильные названия модели и т.п., то файл нужно дополнить руками. 
b)	Объекты в файле json для которых нет необходимости в автогенерации нужно удалить(закомментировать) или установить признак AlwaysSkip.
c)	Отредактированный файл нужно разместить в СodeGenerator\templates\collector.json (Я специально создаю его в другом месте, что бы исключить возможность перезатереть при запуске тестов
3)	Генерация кода приложения: запуск теста GeneratorCommand.GenerateAll() логика работы которого приблизительно следующая:  
a)	Читается файл collector.json.
b)	По каждому объекту в файле собирается дополнительная информация (состав колонок и поля формы) 
c)	Код нужных классов создается по шаблону. На каждый объект следующие классы:
i)	Контроллер табличной формы
ii)	Вью табличной формы  
iii)	Контроллер формы
iv)	Вью формы
v)	Js тест
vi)	Для удобства отладки формируется так же меню.

d)	Файлы сохраняются в том же месте и с тем же именем как в исходном приложении (спасибо, что мы выбрали MVC) 
e)	В случае необходимости предусмотрена возможность отключить генерацию любого из классов в списке выше.

4)	Отладка приложения и запуск тестов (с отладкой все как всегда, берем и делаем)

  И наконец вишенка на торте ТЕСТЫ. 
  
Для тестирования я использовал end to end тесты, написанные на Cypress (чем мне нравится cypress так тем, что дает стабильные результаты). 
В шаблоне приведены простейшие тесты на табличную форму (своего рода Smoke Tests).

   •	Проверка наличия заголовка
   
   •	Проверка что при пустых значениях фильтра в таблице есть данные
   
   •	Проверка, что, установив перекрестные даты в фильтре получаешь пустую таблицу
   
В тестах используется та особенность, что в шаблоне большинство таблиц имеет дополнительный фильтр по датам. Это позволяет задать значения фильтра таким образом, чтобы получить пустую таблицу. 
Для тех таблиц для которых даты нет, текст теста нужно поправить в процессе тестирования (нужно конечно сделать это автоматом создавать чуть другой тест для этого случая, но руки не дошли.)

Тесты можно запустить из Visual Studio через Test Explorer. ![image](https://github.com/SergiyShest/DevExpressToDevExtremeMigrate/assets/28971150/972e95d6-0efb-4963-87eb-77c041e5decd)

 
