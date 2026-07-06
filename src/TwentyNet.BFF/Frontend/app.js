const API_BASE = '';
const HUB_URL = '/hubs/workspace';

const LS_TOKEN = 'twentynet_token';
const LS_REFRESH = 'twentynet_refresh';
const LS_WORKSPACE = 'twentynet_workspace';
const LS_USER = 'twentynet_user';

let connection = null;

function $(id) {
  return document.getElementById(id);
}

function getAuth() {
  return {
    token: localStorage.getItem(LS_TOKEN),
    workspaceId: localStorage.getItem(LS_WORKSPACE),
    userId: localStorage.getItem(LS_USER)
  };
}

function setAuth(data) {
  localStorage.setItem(LS_TOKEN, data.accessToken);
  localStorage.setItem(LS_REFRESH, data.refreshToken);
  localStorage.setItem(LS_WORKSPACE, data.workspaceId);
  localStorage.setItem(LS_USER, data.userId);
}

function clearAuth() {
  localStorage.removeItem(LS_TOKEN);
  localStorage.removeItem(LS_REFRESH);
  localStorage.removeItem(LS_WORKSPACE);
  localStorage.removeItem(LS_USER);
}

function isLoggedIn() {
  return !!getAuth().token;
}

async function api(path, options = {}) {
  const url = `${API_BASE}${path}`;
  const headers = {
    'Content-Type': 'application/json',
    ...(options.headers || {})
  };

  const token = getAuth().token;
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const response = await fetch(url, {
    ...options,
    headers
  });

  if (response.status === 401) {
    clearAuth();
    navigate('login');
    throw new Error('Session expired. Please log in again.');
  }

  if (!response.ok) {
    let message = `Request failed: ${response.status}`;
    try {
      const body = await response.json();
      message = body.title || body.detail || JSON.stringify(body);
    } catch {
      message = await response.text() || message;
    }
    throw new Error(message);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

function showError(message) {
  const main = $('main');
  const el = document.createElement('div');
  el.className = 'error';
  el.textContent = message;
  main.prepend(el);
  setTimeout(() => el.remove(), 5000);
}

function showSuccess(message) {
  const main = $('main');
  const el = document.createElement('div');
  el.className = 'success';
  el.textContent = message;
  main.prepend(el);
  setTimeout(() => el.remove(), 3000);
}

function updateHeader() {
  const header = $('header');
  if (isLoggedIn()) {
    header.style.display = 'flex';
  } else {
    header.style.display = 'none';
  }
}

function startSignalR() {
  if (typeof signalR === 'undefined') {
    console.warn('SignalR client not loaded');
    return;
  }

  if (connection) {
    connection.stop();
  }

  const token = getAuth().token;
  if (!token) return;

  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${HUB_URL}?access_token=${encodeURIComponent(token)}`)
    .withAutomaticReconnect()
    .build();

  connection.on('ObjectRecordChanged', (objectName, recordId, changeType) => {
    console.log('Real-time update:', objectName, recordId, changeType);
    const hash = window.location.hash;
    if (hash.startsWith('#companies') && objectName === 'Company') {
      loadCompanies();
    } else if (hash.startsWith('#people') && objectName === 'Person') {
      loadPeople();
    } else if (hash.startsWith('#company/') || hash.startsWith('#person/')) {
      const parts = hash.split('/');
      if (parts.length >= 2 && parts[1] === recordId) {
        loadDetail(hash);
      }
    }
  });

  connection.onreconnecting(() => updateConnectionStatus(false, 'Reconnecting...'));
  connection.onreconnected(() => updateConnectionStatus(true));
  connection.onclose(() => updateConnectionStatus(false));

  connection.start()
    .then(() => updateConnectionStatus(true))
    .catch(err => {
      console.error('SignalR error:', err);
      updateConnectionStatus(false);
    });
}

function updateConnectionStatus(connected, text) {
  const status = $('connectionStatus');
  if (!status) return;

  if (connected) {
    status.textContent = 'SignalR: online';
    status.className = 'status connected';
  } else {
    status.textContent = `SignalR: ${text || 'offline'}`;
    status.className = 'status disconnected';
  }
}

function stopSignalR() {
  if (connection) {
    connection.stop();
    connection = null;
  }
  updateConnectionStatus(false);
}

function navigate(view) {
  window.location.hash = view;
}

function renderLogin() {
  updateHeader();
  $('main').innerHTML = `
    <div class="card" style="max-width:420px;margin:3rem auto">
      <h2>Welcome to TwentyNet</h2>
      <p class="small">Sign in with your email, password and workspace.</p>
      <form id="loginForm">
        <div class="form-group">
          <label for="email">Email</label>
          <input type="email" id="email" required placeholder="user@example.com" />
        </div>
        <div class="form-group">
          <label for="password">Password</label>
          <input type="password" id="password" required placeholder="Password" />
        </div>
        <div class="form-group">
          <label for="workspaceId">Workspace ID</label>
          <input type="text" id="workspaceId" required placeholder="00000000-0000-0000-0000-000000000000" />
        </div>
        <button type="submit" class="btn btn-primary" style="width:100%">Sign In</button>
      </form>
      <p style="margin-top:1rem;text-align:center">Don't have an account? <span class="link" id="showRegister">Register</span></p>
    </div>
  `;

  $('loginForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    try {
      const data = await api('/api/auth/login', {
        method: 'POST',
        body: JSON.stringify({
          email: $('email').value,
          password: $('password').value,
          workspaceId: $('workspaceId').value
        })
      });
      setAuth(data);
      startSignalR();
      navigate('companies');
    } catch (err) {
      showError(err.message);
    }
  });

  $('showRegister').addEventListener('click', () => navigate('register'));
}

function renderRegister() {
  updateHeader();
  $('main').innerHTML = `
    <div class="card" style="max-width:420px;margin:3rem auto">
      <h2>Create account</h2>
      <form id="registerForm">
        <div class="form-group">
          <label for="email">Email</label>
          <input type="email" id="email" required />
        </div>
        <div class="form-group">
          <label for="password">Password</label>
          <input type="password" id="password" required minlength="6" />
        </div>
        <div class="form-group">
          <label for="firstName">First name</label>
          <input type="text" id="firstName" required />
        </div>
        <div class="form-group">
          <label for="lastName">Last name</label>
          <input type="text" id="lastName" required />
        </div>
        <div class="form-group">
          <label for="workspaceName">Workspace name</label>
          <input type="text" id="workspaceName" required />
        </div>
        <button type="submit" class="btn btn-primary" style="width:100%">Register</button>
      </form>
      <p style="margin-top:1rem;text-align:center">Already have an account? <span class="link" id="showLogin">Sign in</span></p>
    </div>
  `;

  $('registerForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    try {
      const data = await api('/api/auth/register', {
        method: 'POST',
        body: JSON.stringify({
          email: $('email').value,
          password: $('password').value,
          firstName: $('firstName').value,
          lastName: $('lastName').value,
          workspaceName: $('workspaceName').value
        })
      });
      setAuth(data);
      startSignalR();
      navigate('companies');
    } catch (err) {
      showError(err.message);
    }
  });

  $('showLogin').addEventListener('click', () => navigate('login'));
}

async function loadCompanies() {
  try {
    const result = await api('/api/companies?take=50');
    const list = $('companiesList');
    if (!list) return;

    if (!result.items || result.items.length === 0) {
      list.innerHTML = '<li class="empty list-item">No companies yet.</li>';
      return;
    }

    list.innerHTML = result.items.map(c => `
      <li class="list-item">
        <div>
          <a href="#company/${c.id}">${escapeHtml(c.name)}</a>
          <div class="small">${c.domainName ? escapeHtml(c.domainName) : ''} ${c.address ? '· ' + escapeHtml(c.address) : ''}</div>
        </div>
      </li>
    `).join('');
  } catch (err) {
    showError(err.message);
  }
}

function renderCompanies() {
  $('main').innerHTML = `
    <div class="toolbar">
      <h1>Companies</h1>
      <button class="btn btn-primary" id="newCompanyBtn">+ New Company</button>
    </div>
    <div id="newCompanyForm"></div>
    <ul id="companiesList" class="list"></ul>
  `;

  $('newCompanyBtn').addEventListener('click', () => {
    const container = $('newCompanyForm');
    container.innerHTML = `
      <div class="card">
        <h2>New Company</h2>
        <form id="companyForm">
          <div class="form-group">
            <label for="name">Name *</label>
            <input type="text" id="name" required />
          </div>
          <div class="form-group">
            <label for="domainName">Domain</label>
            <input type="text" id="domainName" />
          </div>
          <div class="form-group">
            <label for="address">Address</label>
            <input type="text" id="address" />
          </div>
          <button type="submit" class="btn btn-primary">Create</button>
          <button type="button" class="btn btn-secondary" id="cancelCompany">Cancel</button>
        </form>
      </div>
    `;

    $('companyForm').addEventListener('submit', async (e) => {
      e.preventDefault();
      try {
        await api('/api/companies', {
          method: 'POST',
          body: JSON.stringify({
            name: $('name').value,
            domainName: $('domainName').value || null,
            address: $('address').value || null
          })
        });
        showSuccess('Company created');
        container.innerHTML = '';
        loadCompanies();
      } catch (err) {
        showError(err.message);
      }
    });

    $('cancelCompany').addEventListener('click', () => container.innerHTML = '');
  });

  loadCompanies();
}

async function loadPeople() {
  try {
    const result = await api('/api/people?take=50');
    const list = $('peopleList');
    if (!list) return;

    if (!result.items || result.items.length === 0) {
      list.innerHTML = '<li class="empty list-item">No people yet.</li>';
      return;
    }

    list.innerHTML = result.items.map(p => `
      <li class="list-item">
        <div>
          <a href="#person/${p.id}">${escapeHtml(p.firstName)} ${escapeHtml(p.lastName)}</a>
          <div class="small">${p.email ? escapeHtml(p.email) : ''} ${p.phone ? '· ' + escapeHtml(p.phone) : ''}</div>
        </div>
      </li>
    `).join('');
  } catch (err) {
    showError(err.message);
  }
}

function renderPeople() {
  $('main').innerHTML = `
    <div class="toolbar">
      <h1>People</h1>
      <button class="btn btn-primary" id="newPersonBtn">+ New Person</button>
    </div>
    <div id="newPersonForm"></div>
    <ul id="peopleList" class="list"></ul>
  `;

  $('newPersonBtn').addEventListener('click', () => {
    const container = $('newPersonForm');
    container.innerHTML = `
      <div class="card">
        <h2>New Person</h2>
        <form id="personForm">
          <div class="form-group">
            <label for="firstName">First name *</label>
            <input type="text" id="firstName" required />
          </div>
          <div class="form-group">
            <label for="lastName">Last name *</label>
            <input type="text" id="lastName" required />
          </div>
          <div class="form-group">
            <label for="email">Email</label>
            <input type="email" id="email" />
          </div>
          <div class="form-group">
            <label for="phone">Phone</label>
            <input type="text" id="phone" />
          </div>
          <div class="form-group">
            <label for="companyId">Company ID (optional)</label>
            <input type="text" id="companyId" placeholder="00000000-0000-0000-0000-000000000000" />
          </div>
          <button type="submit" class="btn btn-primary">Create</button>
          <button type="button" class="btn btn-secondary" id="cancelPerson">Cancel</button>
        </form>
      </div>
    `;

    $('personForm').addEventListener('submit', async (e) => {
      e.preventDefault();
      try {
        await api('/api/people', {
          method: 'POST',
          body: JSON.stringify({
            firstName: $('firstName').value,
            lastName: $('lastName').value,
            email: $('email').value || null,
            phone: $('phone').value || null,
            companyId: $('companyId').value || null
          })
        });
        showSuccess('Person created');
        container.innerHTML = '';
        loadPeople();
      } catch (err) {
        showError(err.message);
      }
    });

    $('cancelPerson').addEventListener('click', () => container.innerHTML = '');
  });

  loadPeople();
}

function renderDetail(type, id) {
  const title = type === 'company' ? 'Company' : 'Person';
  $('main').innerHTML = `
    <div class="toolbar">
      <h1 id="detailTitle">${title}</h1>
      <a href="#${type === 'company' ? 'companies' : 'people'}" class="btn btn-secondary">Back</a>
    </div>
    <div id="detailCard" class="card"></div>

    <div class="tabs">
      <button class="tab active" data-tab="notes">Notes</button>
      <button class="tab" data-tab="tasks">Tasks</button>
      <button class="tab" data-tab="timeline">Timeline</button>
      <button class="tab" data-tab="files">Files</button>
    </div>

    <div id="notes" class="tab-content active">
      <div id="notesList"></div>
      <div class="card">
        <h3>Add note</h3>
        <form id="noteForm">
          <div class="form-group">
            <label for="noteTitle">Title</label>
            <input type="text" id="noteTitle" required />
          </div>
          <div class="form-group">
            <label for="noteContent">Content</label>
            <textarea id="noteContent" required></textarea>
          </div>
          <button type="submit" class="btn btn-primary">Add Note</button>
        </form>
      </div>
    </div>

    <div id="tasks" class="tab-content">
      <div id="tasksList"></div>
      <div class="card">
        <h3>Add task</h3>
        <form id="taskForm">
          <div class="form-group">
            <label for="taskTitle">Title</label>
            <input type="text" id="taskTitle" required />
          </div>
          <div class="form-group">
            <label for="taskDueDate">Due date</label>
            <input type="date" id="taskDueDate" />
          </div>
          <button type="submit" class="btn btn-primary">Add Task</button>
        </form>
      </div>
    </div>

    <div id="timeline" class="tab-content">
      <div id="timelineList"></div>
    </div>

    <div id="files" class="tab-content">
      <div id="filesList"></div>
    </div>
  `;

  document.querySelectorAll('.tab').forEach(tab => {
    tab.addEventListener('click', () => {
      document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
      document.querySelectorAll('.tab-content').forEach(c => c.classList.remove('active'));
      tab.classList.add('active');
      $(tab.dataset.tab).classList.add('active');
    });
  });

  loadDetailData(type, id);

  $('noteForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    try {
      await api(`/api/${type}s/${id}/notes`, {
        method: 'POST',
        body: JSON.stringify({
          title: $('noteTitle').value,
          content: $('noteContent').value
        })
      });
      $('noteForm').reset();
      loadNotes(type, id);
    } catch (err) {
      showError(err.message);
    }
  });

  $('taskForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    try {
      await api(`/api/${type}s/${id}/tasks`, {
        method: 'POST',
        body: JSON.stringify({
          title: $('taskTitle').value,
          dueDate: $('taskDueDate').value ? new Date($('taskDueDate').value).toISOString() : null
        })
      });
      $('taskForm').reset();
      loadTasks(type, id);
    } catch (err) {
      showError(err.message);
    }
  });
}

async function loadDetailData(type, id) {
  try {
    const data = await api(`/api/${type}s/${id}`);
    const card = $('detailCard');
    if (type === 'company') {
      $('detailTitle').textContent = data.name;
      card.innerHTML = `
        <h2>${escapeHtml(data.name)}</h2>
        <p class="small">Domain: ${data.domainName ? escapeHtml(data.domainName) : '-'}</p>
        <p class="small">Address: ${data.address ? escapeHtml(data.address) : '-'}</p>
        <p class="small">Workspace: ${data.workspaceId}</p>
      `;
    } else {
      $('detailTitle').textContent = `${data.firstName} ${data.lastName}`;
      card.innerHTML = `
        <h2>${escapeHtml(data.firstName)} ${escapeHtml(data.lastName)}</h2>
        <p class="small">Email: ${data.email ? escapeHtml(data.email) : '-'}</p>
        <p class="small">Phone: ${data.phone ? escapeHtml(data.phone) : '-'}</p>
        <p class="small">Company ID: ${data.companyId || '-'}</p>
      `;
    }

    loadNotes(type, id);
    loadTasks(type, id);
    loadTimeline(type, id);
    loadFiles(type, id);
  } catch (err) {
    showError(err.message);
  }
}

async function loadNotes(type, id) {
  const container = $('notesList');
  try {
    const notes = await api(`/api/${type}s/${id}/notes`);
    if (!notes || notes.length === 0) {
      container.innerHTML = '<p class="empty">No notes.</p>';
      return;
    }
    container.innerHTML = notes.map(n => `
      <div class="card">
        <h4>${escapeHtml(n.title)}</h4>
        <p>${escapeHtml(n.content)}</p>
        <p class="small">By ${n.createdByUserId} · ${new Date(n.createdAt).toLocaleString()}</p>
      </div>
    `).join('');
  } catch (err) {
    container.innerHTML = `<p class="empty">${err.message}</p>`;
  }
}

async function loadTasks(type, id) {
  const container = $('tasksList');
  try {
    const tasks = await api(`/api/${type}s/${id}/tasks`);
    if (!tasks || tasks.length === 0) {
      container.innerHTML = '<p class="empty">No tasks.</p>';
      return;
    }
    container.innerHTML = tasks.map(t => `
      <div class="card">
        <h4>${escapeHtml(t.title)}</h4>
        <p class="small">Status: ${t.status} · Due: ${t.dueDate ? new Date(t.dueDate).toLocaleDateString() : '-'}</p>
      </div>
    `).join('');
  } catch (err) {
    container.innerHTML = `<p class="empty">${err.message}</p>`;
  }
}

async function loadTimeline(type, id) {
  const container = $('timelineList');
  try {
    const items = await api(`/api/${type}s/${id}/timeline`);
    if (!items || items.length === 0) {
      container.innerHTML = '<p class="empty">No timeline activity.</p>';
      return;
    }
    container.innerHTML = items.map(i => `
      <div class="card">
        <h4>${escapeHtml(i.title)}</h4>
        <p>${i.description ? escapeHtml(i.description) : ''}</p>
        <p class="small">${i.activityType} · ${new Date(i.createdAt).toLocaleString()}</p>
      </div>
    `).join('');
  } catch (err) {
    container.innerHTML = `<p class="empty">${err.message}</p>`;
  }
}

async function loadFiles(type, id) {
  const container = $('filesList');
  try {
    const files = await api(`/api/${type}s/${id}/attachments`);
    if (!files || files.length === 0) {
      container.innerHTML = '<p class="empty">No files.</p>';
      return;
    }
    container.innerHTML = files.map(f => `
      <div class="card">
        <h4>${escapeHtml(f.name)}</h4>
        <p class="small">${f.mimeType} · ${f.size} bytes</p>
      </div>
    `).join('');
  } catch (err) {
    container.innerHTML = `<p class="empty">${err.message}</p>`;
  }
}

function escapeHtml(text) {
  if (!text) return '';
  const div = document.createElement('div');
  div.textContent = text;
  return div.innerHTML;
}

function loadSignalRScript() {
  return new Promise((resolve, reject) => {
    if (typeof signalR !== 'undefined') {
      resolve();
      return;
    }

    const script = document.createElement('script');
    script.src = 'https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/8.0.7/signalr.min.js';
    script.integrity = 'sha512-7SRCYdfgPwRiRenioffi48mTBmnaX1rC27noiRTToVjKmqaXKwg6X7Rx6i7JKiBCbseD1KB9fkleCsv2GxE6+A==';
    script.crossOrigin = 'anonymous';
    script.referrerPolicy = 'no-referrer';
    script.onload = resolve;
    script.onerror = () => reject(new Error('Failed to load SignalR client'));
    document.head.appendChild(script);
  });
}

function router() {
  const hash = window.location.hash.replace('#', '') || 'companies';

  if (!isLoggedIn() && hash !== 'register') {
    renderLogin();
    return;
  }

  updateHeader();

  if (hash === 'login') {
    renderLogin();
  } else if (hash === 'register') {
    renderRegister();
  } else if (hash === 'companies') {
    renderCompanies();
  } else if (hash === 'people') {
    renderPeople();
  } else if (hash.startsWith('company/')) {
    renderDetail('company', hash.split('/')[1]);
  } else if (hash.startsWith('person/')) {
    renderDetail('person', hash.split('/')[1]);
  } else {
    renderCompanies();
  }
}

async function init() {
  $('logoutBtn').addEventListener('click', () => {
    clearAuth();
    stopSignalR();
    navigate('login');
  });

  try {
    await loadSignalRScript();
    if (isLoggedIn()) {
      startSignalR();
    }
  } catch (err) {
    console.warn(err);
  }

  window.addEventListener('hashchange', router);
  router();
}

init();
