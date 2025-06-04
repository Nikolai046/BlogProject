document.addEventListener("DOMContentLoaded", function () {
    // Создаем DIV для плавающего окна
    let floatingWindow = document.createElement('div');
    floatingWindow.id = 'floating-window';
    floatingWindow.className = 'floating-window d-none';

    // Внутренний контент окна
    floatingWindow.innerHTML = `
    <div class="window-content">
        <p>Пользователь: <span id="username"></span></p>
        <p>Роль: <span id="user-role"></span></p>
        <p>Email: <span id="user-email"></span></p>
        <p>Количество статей: <span id="article-count"></span></p>
    </div>
    `;

    // Берем данные из существующих переменных
    //let fullname = '@fullname'; // Предполагается, что на сервере или скриптом уже объявлена переменная с полным именем
    //let role = '@role';         // Аналогично для роли
    //let email = '@email';       // Адрес электронной почты
    //let articleCount = '@articleCount'; // Количество статей

    // Найдем элементы внутри плавающего окна и заполним их данными
    let usernameSpan = floatingWindow.querySelector('#username');
    let userRoleSpan = floatingWindow.querySelector('#user-role');
    let userEmailSpan = floatingWindow.querySelector('#user-email');
    let articleCountSpan = floatingWindow.querySelector('#article-count');

    usernameSpan.textContent = fullname;
    userRoleSpan.textContent = role;
    userEmailSpan.textContent = email;
    articleCountSpan.textContent = articleCount;

    // Добавляем CSS стили для плавающего окна
    let cssStyles = `
    <style>
        .floating-window {
            position: fixed;
        bottom: 10px; /* Располагается внизу страницы */
        left: 10px; /* Слева */
        width: 200px;
        max-width: calc(100vw - 20px); /* Авто ширина для мобильных устройств */
        padding: 10px;
        background-color: rgba(255, 255, 255, 0.9); /* Белый полупрозрачный фон */
        border-radius: 5px;
        font-size: 14px;
        line-height: 1.5em;
        box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
        transition: opacity 0.3s ease-in-out;
        pointer-events: none; /* Невзаимодействующее окно */
        cursor: default;
            }

        .window-content {
            word - wrap: break-word;
            }

        .floating-window.show {
            opacity: 1;
        pointer-events: auto; /* Восстанавливаем взаимодействия */
            }

        .floating-window.d-none {
            opacity: 0;
        visibility: hidden;
            }
    </style>
    `;

    // Добавляем CSS стили в head
    document.head.insertAdjacentHTML('beforeend', cssStyles);

    // Добавляем плавающее окно в документ
    document.body.appendChild(floatingWindow);

    // Показываем окно плавно
    floatingWindow.classList.remove('d-none'); // Первоначально скрытое окно
    floatingWindow.classList.add('show'); // Покажем окно
});
