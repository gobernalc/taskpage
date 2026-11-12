import { Routes } from '@angular/router';

/** Root application routes with lazy-loaded feature shells. */
export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'tasks' },
  {
    path: 'tasks',
    loadComponent: () =>
      import('./features/tasks/task-list/task-list').then((module) => module.TaskList),
  },
  {
    path: 'tasks/new',
    loadComponent: () =>
      import('./features/tasks/task-create/task-create').then((module) => module.TaskCreate),
  },
  {
    path: 'tasks/:id/edit',
    loadComponent: () =>
      import('./features/tasks/task-edit/task-edit').then((module) => module.TaskEdit),
  },
  {
    path: 'tasks/:id',
    loadComponent: () =>
      import('./features/tasks/task-detail/task-detail').then((module) => module.TaskDetail),
  },
  {
    path: 'tasks/:id',
    loadComponent: () =>
      import('./features/tasks/task-detail/task-detail').then((module) => module.TaskDetail),
  },
  {
    path: 'users',
    loadComponent: () =>
      import('./features/users/user-list/user-list').then((module) => module.UserList),
  },
  {
    path: 'users/new',
    loadComponent: () =>
      import('./features/users/user-form/user-form').then((module) => module.UserForm),
  },
  {
    path: 'users/:id/edit',
    loadComponent: () =>
      import('./features/users/user-form/user-form').then((module) => module.UserForm),
  },
  { path: '**', redirectTo: 'tasks' },
];
