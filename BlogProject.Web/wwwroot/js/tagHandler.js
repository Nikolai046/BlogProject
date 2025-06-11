document.addEventListener('DOMContentLoaded', function () {
    const tagInput = document.getElementById('tagInput');
    const tagDisplay = document.getElementById('tagDisplay');
    const tagListInput = document.getElementById('tagList');

    // Проверяем наличие элементов
    if (!tagInput || !tagDisplay || !tagListInput) {
        console.error('Один или несколько элементов не найдены:', { tagInput, tagDisplay, tagListInput });
        return;
    }

    // Получаем начальные теги из глобальной переменной, с fallback на пустой массив
    const initialTags = window.initialTags || [];
    console.log('initialTags:', initialTags); // Для отладки

    const upperCaseInitialTags = initialTags.map(tag => tag.toUpperCase());
    let tags = [...new Set(upperCaseInitialTags)]; // Убираем дубликаты

    function renderTags() {
        tagDisplay.innerHTML = '';
        tags.forEach((tag, index) => {
            const tagElement = document.createElement('span');
            tagElement.className = 'badge bg-info text-white rounded-pill d-inline-block';
            tagElement.innerHTML = `${tag} <span class="ms-1" data-index="${index}" style="cursor: pointer;">×</span>`;
            tagDisplay.appendChild(tagElement);
        });
        tagListInput.value = JSON.stringify(tags); // Сохраняем теги в скрытое поле
    }

    // Обработчик ввода тегов
    tagInput.addEventListener('input', () => {
        const currentWords = tagInput.value.split(/\s+/).filter(Boolean);
        const upperCaseWords = currentWords.map(word => word.toUpperCase());
        tags = [...new Set(upperCaseWords)]; // Обновляем теги
        renderTags();
    });

    // Удаление тега по клику
    tagDisplay.addEventListener('click', function (e) {
        if (e.target && e.target.matches('span[data-index]')) {
            const index = parseInt(e.target.getAttribute('data-index'), 10);
            tags.splice(index, 1);
            tagInput.value = tags.join(' ') + (tags.length > 0 ? ' ' : '');
            renderTags();
        }
    });

    // Инициализация тегов
    if (tags.length > 0) {
        tagInput.value = tags.join(' ') + ' ';
        renderTags();
    }
});