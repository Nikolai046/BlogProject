document.addEventListener('DOMContentLoaded', function () {
    const textareas = document.querySelectorAll('.auto-resize');

    textareas.forEach(textarea => {
        // Инициализация высоты
        adjustHeight(textarea);

        // Обработчик ввода
        textarea.addEventListener('input', function () {
            adjustHeight(this);
        });

        // Обработчик для Enter - предотвращаем автоматическую прокрутку браузера
        textarea.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                // Запоминаем текущую позицию скролла
                const currentScroll = window.scrollY;

                // После обработки Enter восстанавливаем позицию, если не нужна прокрутка
                setTimeout(() => {
                    if (isCursorVisible(this)) {
                        window.scrollTo(0, currentScroll);
                    } else {
                        // Прокручиваем только если курсор действительно не виден
                        scrollToCursor(this);
                    }
                }, 0);
            }
        });
    });

    function adjustHeight(textarea) {
        // Сбрасываем высоту для правильного расчета
        textarea.style.height = 'auto';

        // Устанавливаем новую высоту
        const newHeight = textarea.scrollHeight;
        textarea.style.height = newHeight + 'px';
    }

    function isCursorVisible(textarea) {
        // Получаем позицию курсора
        const cursorPos = textarea.selectionStart;

        // Создаем временный элемент для определения позиции курсора
        const temp = document.createElement('div');
        const style = window.getComputedStyle(textarea);

        // Копируем стили textarea
        temp.style.cssText = `
position: absolute;
top: ${textarea.offsetTop}px;
left: ${textarea.offsetLeft}px;
width: ${textarea.clientWidth}px;
height: auto;
padding: ${style.padding};
border: ${style.border};
font: ${style.font};
line-height: ${style.lineHeight};
white-space: pre-wrap;
word-wrap: break-word;
visibility: hidden;
overflow-wrap: break-word;
`;

        // Вставляем текст до курсора
        temp.textContent = textarea.value.substring(0, cursorPos);

        // Добавляем маркер для курсора
        const marker = document.createElement('span');
        marker.textContent = '|';
        temp.appendChild(marker);

        // Добавляем временный элемент в родителя textarea
        textarea.parentNode.appendChild(temp);

        // Получаем позицию маркера
        const markerRect = marker.getBoundingClientRect();
        const markerTop = markerRect.top + window.scrollY;

        // Удаляем временный элемент
        temp.remove();

        // Проверяем, виден ли курсор в окне просмотра
        const windowTop = window.scrollY;
        const windowBottom = window.scrollY + window.innerHeight;

        // Добавляем буфер для комфортного просмотра (100px сверху и снизу)
        const buffer = 100;

        return markerTop > (windowTop + buffer) && markerTop < (windowBottom - buffer);
    }

    function scrollToCursor(textarea) {
        // Получаем позицию курсора
        const cursorPos = textarea.selectionStart;

        // Создаем временный элемент для определения позиции курсора
        const temp = document.createElement('div');
        const style = window.getComputedStyle(textarea);

        // Копируем стили textarea
        temp.style.cssText = `
position: absolute;
top: ${textarea.offsetTop}px;
left: ${textarea.offsetLeft}px;
width: ${textarea.clientWidth}px;
height: auto;
padding: ${style.padding};
border: ${style.border};
font: ${style.font};
line-height: ${style.lineHeight};
white-space: pre-wrap;
word-wrap: break-word;
visibility: hidden;
overflow-wrap: break-word;
`;

        // Вставляем текст до курсора
        temp.textContent = textarea.value.substring(0, cursorPos);

        // Добавляем маркер для курсора
        const marker = document.createElement('span');
        marker.textContent = '|';
        temp.appendChild(marker);

        // Добавляем временный элемент в родителя textarea
        textarea.parentNode.appendChild(temp);

        // Получаем позицию маркера
        const markerRect = marker.getBoundingClientRect();
        const markerTop = markerRect.top + window.scrollY;

        // Удаляем временный элемент
        temp.remove();

        // Определяем целевую позицию прокрутки
        // Ставим курсор примерно в центре экрана
        const targetScroll = markerTop - (window.innerHeight / 2);

        // Плавная прокрутка
        window.scrollTo({
            top: Math.max(0, targetScroll),
            behavior: 'smooth'
        });
    }
});