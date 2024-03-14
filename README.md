
Стенд для демонстрации возможности автоматической конвертации слоя UI написанного на Devexpress в любой другой. 
Здесь это Devextreme, но сие не принципиально.Задача возникла в ходе импортозамещения.
Не предлагаю ничего нового, известный подход, приложенный к конкретной ситуации. 

Совсем кратко задачу можно описать так:

Есть: Исходный проект с множеством таблиц и форм.

Задача: Нужно в целевом проекте создать аналогичные таблицы и формы по другой технологии. По сути много однотипных классов и файлов, отличающихся заранее известными частями. 

Решение: 
1.	Автоматически собрать необходимую информацию из исходного проекта в файл. 
2.	Дополнить этот файл в той части которую не получилось собрать автоматически. 
3.	Создавать нужные файлы по шаблону заменяя подстановочные символы в шаблоне значениями из файла. Полученные файлы могут быть если необходимо доработаны. 


 
В студии есть встроенный кодогенератор, он реализует похожую задачу. Я изначально рассматривал использование его. Но передумал. Получается много сложнее.  Кроме того, генератор в студии предполагается использовать постоянно, мой генератор нужен одноразово, что бы создать заготовку кода. Хотя... 

В прототипе 3 приложения:
1) Source: Файлы исходного приложения на net framework 4.7 + MVC и Devexpress.Приложение не собирается потому, что я удалил все лишнее и оставил только необходимые файлы. Когда-то я нашел это приложение как пример для изучения Devexpress. 
 2) Target: Целевое приложение на .NET MVC. Создано по шаблону в VisualStudio.  В него добавлен проект  Core в котором  в папке Entity помещены те же файлы Entity что и в исходном приложении. Что бы в не заморачиваться с базой данных там же расположен класс TestDataHelper который возвращает IQueryable<T> имитируя работу с базой  создавая данные случайным образом.  В проект UI добавлены базовые Generic контроллеры и необходимые js файлы
 3) CodeGenerator: Генератор который на основании Devexpress таблиц в исходном проекте создает аналогичные таблицы в целевом проекте, но уже по другой технологии (Devextreme).

Особенности целевого проекта:
То же MVC. В целях упрощения отказались от идеи использовать SPA. Каждая странница должна загружаться отдельно. Так же в целях упрощения было принято решение отказаться от сборщиков и загружать js библиотеки через cdn.
Генератор создает таблицы и карточки аналогичные таковым в исходном проекте по шаблонам. Текст шаблона в примере сильно упрощен. Но общей сути это не поменяло.
 
Для ускорения разработки и гибкости генератор управляется запуском тестов. Это удобно для программ, которые выполняются только разработчиком. 
 Тесты для управления генератором вынесены в отдельный класс GeneratorCommand в котором всего 3 метода (теста)
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

4)	Отладка приложения и запуск тестов (с отладкой все как всегда, берем и делаем) Код для табличной части в 90% случаев может быть оставлен без изменения. С формой редактирования сложнее. Создается по сути заготовка, которая обязательно должна быть доработана.   

  И наконец вишенка на торте ТЕСТЫ. 
  
Для тестирования я использовал end to end тесты, написанные на Cypress (чем мне нравится cypress так тем, что дает стабильные результаты). 
В шаблоне приведены простейшие тесты на табличную форму (своего рода Smoke Tests).

   •	Проверка наличия заголовка
   
   •	Проверка что при пустых значениях фильтра в таблице есть данные
   
   •	Проверка, что, установив перекрестные даты в фильтре получаешь пустую таблицу
   
В тестах используется та особенность, что в шаблоне большинство таблиц имеет дополнительный фильтр по датам. Это позволяет задать значения фильтра таким образом, чтобы получить пустую таблицу. 
Для тех таблиц для которых даты нет, текст теста нужно поправить в процессе тестирования (нужно конечно сделать это автоматом создавать чуть другой тест для этого случая, но руки не дошли.)

Тесты можно запустить из Visual Studio через Test Explorer. ![image](https://github.com/SergiyShest/DevExpressToDevExtremeMigrate/assets/28971150/972e95d6-0efb-4963-87eb-77c041e5decd)

 
