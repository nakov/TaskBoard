# TaskBoard C# App + RESTful API + Desktop Client + Android Client
The JS app "Task Board" holds a board of tasks (in Trello style). Each task consists of title + description. Tasks are organized in boards, which are displayed as columns (sections): Open, In Progress, Done. The app supports the following operations:
 - Home page (view tasks count + menu): `/`
 - View the boards with tasks: `/boards`
 - Search tasks form: `/tasks/search`
 - Search tasks by keyword: `/tasks/search/:keyword`
 - View task details (by id): `/tasks/view/:id`
 - Add new task (title + description): `/tasks/create`
 - Edit task / move to board: `/tasks/edit/:id`

## Implementation Details

The app is based on .NET 5, ASP.NET MVC Core + Entity Framework Core + MS SQL Server.

## RESTful API

The following endpoints are supported:
 - `GET /api` - list all API endpoints
 - `GET /api/tasks` - list all tasks
 - `GET /api/tasks/:id` - returns a task by given `id`
 - `GET /api/tasks/search/:keyword` - list all tasks matching given keyword
 - `GET /api/tasks/board/:board` - list tasks by board
 - `POST /api/tasks` - create a new task (post a JSON object in the request body, e.g. `{"title":"Add Tests", "description":"API + UI tests", "board":"Open"}`)
 - `PATCH /api/tasks/:id` - edit task by `id` (send a JSON object in the request body, holding the fields to modify, e.g. `{"title":"changed title", "board":"Done"}`)
 - `DELETE /api/tasks/:id` - delete task by `id`
 - `GET /api/boards` - list all boards

## Screenshots

TODO
