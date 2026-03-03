'use strict';

// ─── State ───────────────────────────────────────────────────────────────────

const App = {
  apiKey: '',
  configId: '',
  apiBaseUrl: '',
  config: null,         // WidgetConfigResponse
  selections: [],       // Array<{ids, labels, values} | null> — seviye başına
  savedSelections: null // populated event'inden gelen restore verisi: { [levelIndex]: {ids, labels, values} }
};

// ─── DOM refs ─────────────────────────────────────────────────────────────────

const $loading = document.getElementById('loading');
const $error   = document.getElementById('error');
const $levels  = document.getElementById('levels');

// ─── Helpers ──────────────────────────────────────────────────────────────────

function showError(msg) {
  $loading.style.display = 'none';
  $levels.style.display  = 'none';
  $error.style.display   = '';
  $error.textContent     = msg;
  resizeFrame();
}

function resizeFrame() {
  const h = document.getElementById('kolaytik-widget').scrollHeight;
  JFCustomWidget.requestFrameResize({ height: h });
}

async function apiFetch(url) {
  const res  = await fetch(url);
  if (!res.ok) throw new Error('HTTP ' + res.status);
  const json = await res.json();
  if (!json.success) throw new Error(json.message || 'API hatası');
  return json.data;
}

// Enum: sayısal (0-3) veya string olabilir
function resolveElementType(val) {
  if (val === 0 || val === 'Dropdown')            return 'Dropdown';
  if (val === 1 || val === 'RadioButton')         return 'RadioButton';
  if (val === 2 || val === 'CheckboxGroup')       return 'CheckboxGroup';
  if (val === 3 || val === 'MultiSelectDropdown') return 'MultiSelectDropdown';
  return 'Dropdown';
}

// ─── Config & Render ──────────────────────────────────────────────────────────

async function loadConfig() {
  try {
    const url  = App.apiBaseUrl + '/api/widget/config'
      + '?api_key='    + encodeURIComponent(App.apiKey)
      + '&config_id='  + encodeURIComponent(App.configId);
    App.config = await apiFetch(url);
    renderLevels();
  } catch (e) {
    showError('Konfigürasyon yüklenemedi: ' + e.message);
  }
}

function renderLevels() {
  $loading.style.display = 'none';
  $levels.innerHTML      = '';
  App.selections         = new Array(App.config.levels.length).fill(null);

  const sorted = App.config.levels.slice().sort((a, b) => a.orderIndex - b.orderIndex);
  sorted.forEach((level, idx) => $levels.appendChild(createLevelDiv(level, idx)));

  $levels.style.display = '';
  loadLevelItems(0, null);
}

function createLevelDiv(level, idx) {
  const div = document.createElement('div');
  div.className = 'kw-level';
  div.id        = 'kw-level-' + idx;
  if (idx > 0) div.style.display = 'none';

  // Label
  const labelEl = document.createElement('div');
  labelEl.className = 'kw-label';
  labelEl.textContent = level.label;
  if (level.isRequired) {
    const req = document.createElement('span');
    req.className   = 'kw-req';
    req.textContent = ' *';
    labelEl.appendChild(req);
  }
  div.appendChild(labelEl);

  // Field container
  const fieldEl = document.createElement('div');
  fieldEl.id = 'kw-field-' + idx;
  div.appendChild(fieldEl);

  // Spinner
  const spinnerEl = document.createElement('div');
  spinnerEl.className   = 'kw-spinner';
  spinnerEl.id          = 'kw-spinner-' + idx;
  spinnerEl.style.display = 'none';
  spinnerEl.textContent = 'Yükleniyor...';
  div.appendChild(spinnerEl);

  // Inline error
  const errEl = document.createElement('div');
  errEl.className = 'kw-error-inline';
  errEl.id        = 'kw-error-' + idx;
  div.appendChild(errEl);

  return div;
}

// ─── Item Loading ─────────────────────────────────────────────────────────────

async function loadLevelItems(levelIdx, parentItemId) {
  const level       = App.config.levels[levelIdx];
  const fieldEl     = document.getElementById('kw-field-'   + levelIdx);
  const spinnerEl   = document.getElementById('kw-spinner-' + levelIdx);
  const errEl       = document.getElementById('kw-error-'   + levelIdx);

  spinnerEl.style.display = '';
  fieldEl.innerHTML       = '';
  errEl.textContent       = '';

  try {
    let url = App.apiBaseUrl + '/api/widget/items'
      + '?api_key=' + encodeURIComponent(App.apiKey)
      + '&list_id=' + encodeURIComponent(level.listId);
    if (parentItemId) url += '&parent_item_id=' + encodeURIComponent(parentItemId);

    const items = await apiFetch(url);
    spinnerEl.style.display = 'none';

    if (!items || items.length === 0) {
      const empty = document.createElement('div');
      empty.className   = 'kw-empty';
      empty.textContent = 'Gösterilecek eleman yok';
      fieldEl.appendChild(empty);
    } else {
      renderField(levelIdx, items);
      // Restore saved selection if available
      if (App.savedSelections && App.savedSelections[levelIdx]) {
        restoreLevelSelection(levelIdx);
      }
    }
  } catch (e) {
    spinnerEl.style.display = 'none';
    errEl.textContent = 'Yüklenemedi: ' + e.message;
  }

  resizeFrame();
}

// ─── Field Rendering ──────────────────────────────────────────────────────────

function renderField(levelIdx, items) {
  const level    = App.config.levels[levelIdx];
  const fieldEl  = document.getElementById('kw-field-' + levelIdx);
  const elType   = resolveElementType(level.elementType);

  switch (elType) {
    case 'MultiSelectDropdown': renderMultiSelectDropdown(levelIdx, level, items, fieldEl); break;
    case 'RadioButton':         renderRadioGroup(levelIdx, level, items, fieldEl);          break;
    case 'CheckboxGroup':       renderCheckboxGroup(levelIdx, level, items, fieldEl);       break;
    default:                    renderDropdown(levelIdx, level, items, fieldEl);
  }
}

function renderDropdown(levelIdx, level, items, container) {
  const select = document.createElement('select');
  select.className = 'kw-select';
  select.id        = 'kw-select-' + levelIdx;

  const ph = document.createElement('option');
  ph.value    = '';
  ph.disabled = true;
  ph.selected = true;
  ph.textContent = level.placeholder || '-- Seçiniz --';
  select.appendChild(ph);

  items.forEach(item => {
    const opt = document.createElement('option');
    opt.value            = item.id;
    opt.textContent      = item.label;
    opt.dataset.label    = item.label;
    opt.dataset.value    = item.value;
    opt.dataset.hasChildren = String(item.hasChildren);
    select.appendChild(opt);
  });

  select.addEventListener('change', function () {
    const opt = this.options[this.selectedIndex];
    if (opt && opt.value) {
      applySelection(levelIdx, [opt.value], [opt.dataset.label], [opt.dataset.value]);
    }
  });

  container.appendChild(select);
}

function renderMultiSelectDropdown(levelIdx, level, items, container) {
  const select = document.createElement('select');
  select.className = 'kw-select';
  select.id        = 'kw-select-' + levelIdx;
  select.multiple  = true;
  select.size      = Math.min(items.length, 6);

  items.forEach(item => {
    const opt = document.createElement('option');
    opt.value               = item.id;
    opt.textContent         = item.label;
    opt.dataset.label       = item.label;
    opt.dataset.value       = item.value;
    opt.dataset.hasChildren = String(item.hasChildren);
    select.appendChild(opt);
  });

  select.addEventListener('change', function () {
    let selected = Array.from(this.selectedOptions);
    if (level.maxSelections && selected.length > level.maxSelections) {
      selected.slice(level.maxSelections).forEach(o => { o.selected = false; });
      selected = selected.slice(0, level.maxSelections);
    }
    const ids    = selected.map(o => o.value);
    const labels = selected.map(o => o.dataset.label);
    const values = selected.map(o => o.dataset.value);
    if (ids.length > 0) {
      applySelection(levelIdx, ids, labels, values);
    } else {
      App.selections[levelIdx] = null;
      clearFrom(levelIdx + 1);
      sendValue();
    }
  });

  container.appendChild(select);
}

function renderRadioGroup(levelIdx, level, items, container) {
  const group = document.createElement('div');
  group.className = 'kw-radio-group';
  group.id        = 'kw-radio-group-' + levelIdx;

  const groupName = 'kw-radio-' + levelIdx + '-' + Date.now();

  items.forEach(item => {
    const lbl = document.createElement('label');
    lbl.className = 'kw-radio-label';

    const input = document.createElement('input');
    input.type              = 'radio';
    input.name              = groupName;
    input.value             = item.id;
    input.className         = 'kw-radio';
    input.dataset.label     = item.label;
    input.dataset.value     = item.value;
    input.dataset.hasChildren = String(item.hasChildren);

    input.addEventListener('change', function () {
      if (this.checked) {
        applySelection(levelIdx, [this.value], [this.dataset.label], [this.dataset.value]);
      }
    });

    const span = document.createElement('span');
    span.textContent = item.label;

    lbl.appendChild(input);
    lbl.appendChild(span);
    group.appendChild(lbl);
  });

  container.appendChild(group);
}

function renderCheckboxGroup(levelIdx, level, items, container) {
  const group = document.createElement('div');
  group.className = 'kw-checkbox-group';
  group.id        = 'kw-checkbox-group-' + levelIdx;

  items.forEach(item => {
    const lbl = document.createElement('label');
    lbl.className = 'kw-checkbox-label';

    const input = document.createElement('input');
    input.type              = 'checkbox';
    input.value             = item.id;
    input.className         = 'kw-checkbox';
    input.dataset.label     = item.label;
    input.dataset.value     = item.value;
    input.dataset.hasChildren = String(item.hasChildren);

    input.addEventListener('change', function (e) {
      collectCheckboxState(levelIdx, level, e.target);
    });

    const span = document.createElement('span');
    span.textContent = item.label;

    lbl.appendChild(input);
    lbl.appendChild(span);
    group.appendChild(lbl);
  });

  container.appendChild(group);
}

// ─── Selection Logic ──────────────────────────────────────────────────────────

function collectCheckboxState(levelIdx, level, changedInput) {
  const group     = document.getElementById('kw-checkbox-group-' + levelIdx);
  const checkboxes = Array.from(group.querySelectorAll('input[type=checkbox]'));
  let checked     = checkboxes.filter(c => c.checked);

  if (level.maxSelections && checked.length > level.maxSelections) {
    if (changedInput) changedInput.checked = false;
    checked = checkboxes.filter(c => c.checked);
  }

  const ids    = checked.map(c => c.value);
  const labels = checked.map(c => c.dataset.label);
  const values = checked.map(c => c.dataset.value);

  if (ids.length > 0) {
    applySelection(levelIdx, ids, labels, values);
  } else {
    App.selections[levelIdx] = null;
    clearFrom(levelIdx + 1);
    sendValue();
  }
}

function applySelection(levelIdx, ids, labels, values) {
  App.selections[levelIdx] = { ids, labels, values };
  clearFrom(levelIdx + 1);

  const nextIdx = levelIdx + 1;
  if (nextIdx < App.config.levels.length) {
    const nextDiv = document.getElementById('kw-level-' + nextIdx);
    if (nextDiv) {
      nextDiv.style.display = '';
      loadLevelItems(nextIdx, ids[0]);
    }
  }

  sendValue();
}

function clearFrom(fromIdx) {
  const len = App.config ? App.config.levels.length : 0;
  for (let i = fromIdx; i < len; i++) {
    App.selections[i] = null;
    const levelDiv  = document.getElementById('kw-level-'   + i);
    const fieldEl   = document.getElementById('kw-field-'   + i);
    const spinnerEl = document.getElementById('kw-spinner-' + i);
    const errEl     = document.getElementById('kw-error-'   + i);

    if (levelDiv  && i > 0) levelDiv.style.display = 'none';
    if (fieldEl)            fieldEl.innerHTML       = '';
    if (spinnerEl)          spinnerEl.style.display = 'none';
    if (errEl)              errEl.textContent       = '';
  }
}

// ─── Send Value ───────────────────────────────────────────────────────────────

function sendValue() {
  if (!App.config) return;

  let valid = true;
  const displayParts = [];

  for (let i = 0; i < App.config.levels.length; i++) {
    const levelDiv = document.getElementById('kw-level-' + i);
    const isVisible = levelDiv && levelDiv.style.display !== 'none';
    const level     = App.config.levels[i];
    const sel       = App.selections[i];

    if (isVisible) {
      if (sel && sel.labels.length > 0) {
        displayParts.push(sel.labels.join(', '));
      } else if (level.isRequired) {
        valid = false;
      }
    }
  }

  const displayValue = displayParts.join(' > ');
  const selData = App.selections
    .map((sel, idx) => sel ? { levelIndex: idx, ids: sel.ids, labels: sel.labels, values: sel.values } : null)
    .filter(Boolean);

  JFCustomWidget.sendSubmit({
    value: displayValue,
    valid: valid,
    customData: JSON.stringify({
      display: displayValue,
      selections: selData
    })
  });
}

// ─── Restore (populated) ──────────────────────────────────────────────────────

function restoreLevelSelection(levelIdx) {
  const saved = App.savedSelections[levelIdx];
  if (!saved || !saved.ids || saved.ids.length === 0) return;

  const level  = App.config.levels[levelIdx];
  const elType = resolveElementType(level.elementType);

  switch (elType) {
    case 'Dropdown': {
      const select = document.getElementById('kw-select-' + levelIdx);
      if (!select) return;
      select.value = saved.ids[0];
      const opt    = select.options[select.selectedIndex];
      if (opt && opt.value) {
        applySelection(levelIdx,
          [opt.value],
          [opt.dataset.label || saved.labels[0]],
          [opt.dataset.value || saved.values[0]]
        );
      }
      break;
    }

    case 'RadioButton': {
      const group = document.getElementById('kw-radio-group-' + levelIdx);
      if (!group) return;
      const radio = Array.from(group.querySelectorAll('input')).find(el => el.value === saved.ids[0]);
      if (radio) {
        radio.checked = true;
        applySelection(levelIdx,
          [radio.value],
          [radio.dataset.label || saved.labels[0]],
          [radio.dataset.value || saved.values[0]]
        );
      }
      break;
    }

    case 'CheckboxGroup': {
      const group = document.getElementById('kw-checkbox-group-' + levelIdx);
      if (!group) return;
      saved.ids.forEach(id => {
        const cb = Array.from(group.querySelectorAll('input')).find(el => el.value === id);
        if (cb) cb.checked = true;
      });
      collectCheckboxState(levelIdx, level, null);
      break;
    }

    case 'MultiSelectDropdown': {
      const select = document.getElementById('kw-select-' + levelIdx);
      if (!select) return;
      saved.ids.forEach(id => {
        const opt = Array.from(select.options).find(o => o.value === id);
        if (opt) opt.selected = true;
      });
      const selected = Array.from(select.selectedOptions);
      const ids      = selected.map(o => o.value);
      const labels   = selected.map(o => o.dataset.label || o.textContent);
      const values   = selected.map(o => o.dataset.value || o.value);
      if (ids.length > 0) applySelection(levelIdx, ids, labels, values);
      break;
    }
  }
}

// ─── JotForm Lifecycle ────────────────────────────────────────────────────────

JFCustomWidget.subscribe('ready', function () {
  App.apiKey     = JFCustomWidget.getWidgetSetting('apiKey')     || '';
  App.configId   = JFCustomWidget.getWidgetSetting('configId')   || '';
  App.apiBaseUrl = (JFCustomWidget.getWidgetSetting('apiBaseUrl') || 'https://api.kolaytik.com').replace(/\/$/, '');

  if (!App.apiKey || !App.configId) {
    showError('Widget yapılandırması eksik: apiKey ve configId gereklidir.');
    return;
  }

  loadConfig();
});

JFCustomWidget.subscribe('populated', function (data) {
  try {
    let customData = data && data.customData;
    if (typeof customData === 'string') customData = JSON.parse(customData);
    if (customData && Array.isArray(customData.selections)) {
      App.savedSelections = {};
      customData.selections.forEach(function (sel) {
        App.savedSelections[sel.levelIndex] = {
          ids:    sel.ids,
          labels: sel.labels,
          values: sel.values
        };
      });
    }
  } catch (_) {
    // Malformed customData — ignore
  }
});
