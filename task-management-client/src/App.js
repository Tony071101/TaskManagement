import { useState, useEffect } from "react";
import TaskForm from "./TaskForm";
import hubConnection from "./signalr";

const API_TASKS_URL = "http://localhost:5001/api/tasks";
const API_USERS_URL = "http://localhost:5001/api/users";

const App = () => {
  const [tasks, setTasks] = useState([]);
  const [users, setUsers] = useState([]);
  const [user, setUser] = useState(null);
  const [editingTask, setEditingTask] = useState(null);
  const [showRegister, setShowRegister] = useState(false);
  const [editingUser, setEditingUser] = useState(false);
  
  useEffect(() => {
    const storedUser = localStorage.getItem("user");
    if (storedUser) {
      setUser(JSON.parse(storedUser));
    }
  }, []);

  useEffect(() => {
    const fetchUsers = async () => {
      try {
        const res = await fetch(API_USERS_URL);
        const data = await res.json();
        setUsers(data);
      } catch (error) {
        console.error("Error fetching users:", error);
      }
    };
  
    fetchUsers();

    if (hubConnection.state === "Disconnected") {
      hubConnection
        .start()
        .then(() => console.log("Connected to SignalR"))
        .catch((err) => console.error("SignalR Connection Error:", err));
    } else {
      console.warn("SignalR already connected or connecting...");
    }

    const handleReceiveUserUpdate = (updatedUsers) => {
        setUsers(updatedUsers);
    };

    hubConnection.on("ReceiveUserUpdate", handleReceiveUserUpdate);

    return () => {
        hubConnection.off("ReceiveUserUpdate", handleReceiveUserUpdate);
    };
  }, []);

  useEffect(() => {
    const fetchTasks = async () => {
      try {
        const res = await fetch(API_TASKS_URL);
        const data = await res.json();
        setTasks(data);
      } catch (error) {
        console.error("Error fetching tasks:", error);
      }
    };
  
    fetchTasks();
  
    if (hubConnection.state === "Disconnected") {
      hubConnection
        .start()
        .then(() => console.log("Connected to SignalR"))
        .catch((err) => console.error("SignalR Connection Error:", err));
    } else {
      console.warn("SignalR already connected or connecting...");
    }
  
    const handleReceiveTaskUpdate = (updatedTasks) => {
      if (Array.isArray(updatedTasks)) {
        setTasks(updatedTasks);
      }
    };
  
    hubConnection.on("ReceiveTaskUpdate", handleReceiveTaskUpdate);
  
    return () => {
      hubConnection.off("ReceiveTaskUpdate", handleReceiveTaskUpdate);
    };
  }, []);

  useEffect(() => {
    const handleReceiveUserUpdate = (updatedUsers) => {
      setUsers(updatedUsers);
   
      // Cập nhật lại thông tin người dùng nếu tên người dùng đã thay đổi
      const updatedUser = updatedUsers.find((u) => u.id === user.id);
      if (updatedUser) {
        setUser(updatedUser);
        localStorage.setItem("user", JSON.stringify(updatedUser));
      }
    };

    if (hubConnection.state === "Disconnected") {
      hubConnection
        .start()
        .then(() => console.log("Connected to SignalR"))
        .catch((err) => console.error("SignalR Connection Error:", err));
    } else {
      console.warn("SignalR already connected or connecting...");
    }
   
    hubConnection.on("ReceiveUserUpdate", handleReceiveUserUpdate);
   
    return () => {
        hubConnection.off("ReceiveUserUpdate", handleReceiveUserUpdate);
    };
  }, [user]);
  

  //Tạo Task (Create)
  const handleCreateTask = async (taskData) => {
    try {
      const token = localStorage.getItem("accessToken");
      if (!token) {
        await refreshAccessToken();
      }
      const res = await fetch(API_TASKS_URL, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(taskData),
      });
      if (!res.ok) throw new Error("Failed to create task");
    } catch (error) {
      console.error("Error creating task:", error);
    }
  };

  //Cập nhật Task (Update)
  const handleUpdateTask = async (updatedTask) => {
    try {
      if (updatedTask === null) {
        // If updatedTask is null, just clear editing state
        setEditingTask(null);
        return;
      }
      const token = localStorage.getItem("accessToken");
      if (!token) {
        await refreshAccessToken(); // Làm mới token nếu cần thiết
      }
      const res = await fetch(`${API_TASKS_URL}/${updatedTask.id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(updatedTask),
      });
      if (!res.ok) throw new Error("Failed to update task");
      setEditingTask(null);
    } catch (error) {
      console.error("Error updating task:", error);
    }
  };

  //Xóa Task (Delete)
  const handleDeleteTask = async (id) => {
    try {
      const token = localStorage.getItem("accessToken");
      if (!token) {
        await refreshAccessToken(); // Làm mới token nếu cần thiết
      }
      await fetch(`${API_TASKS_URL}/${id}`, { method: "DELETE" });
    } catch (error) {
      console.error("Error deleting task:", error);
    }
    setEditingTask(null);
  };

  //Login
  const handleLogin = async (email, password) => {
    try {
        const res = await fetch("http://localhost:5001/api/auth/login", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email, password }),
        });
        if (!res.ok) throw new Error("Login failed");
        const data = await res.json();
        
        // Lưu token và user vào localStorage
        localStorage.setItem("accessToken", data.accessToken);
        localStorage.setItem("refreshToken", data.refreshToken);
        localStorage.setItem("user", JSON.stringify(data.user));

        setUser(data.user);
    } catch (error) {
        console.error("Error logging in:", error);
    }
  };


  const handleRegister = async (name, email, password) => {
    const data = {
      name,
      email,
      password
    };
    try {
      const res = await fetch("http://localhost:5001/api/auth/register", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data),
      });
      if(!res.ok) throw new Error("Register fail!");
      alert("register successed. please login.");
      showRegister(false);
    } catch (error) {
      console.error("Error registering: ", error);
    }
  };

  const handleLogout = () => {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    localStorage.removeItem("user");
    setUser(null);
  };

  const handleUpdateUser = async (updatedUser) => {
    try {
      const token = localStorage.getItem("accessToken");
      if (!token) {
        await refreshAccessToken();
      }
      const res = await fetch(`${API_USERS_URL}/${updatedUser.id}`, {
        method: "PUT",
        headers: { 
          "Content-Type": "application/json",
          "Authorization": `Bearer ${token}`
        },
        body: JSON.stringify(updatedUser),
      });
      if (!res.ok) throw new Error("Failed to update user");
  
      localStorage.setItem("user", JSON.stringify({ ...user, ...updatedUser }));

      setEditingUser(false);
    } catch (error) {
      console.error("Error updating user:", error);
    }
  };

  const refreshAccessToken = async () => {
    const refreshToken = localStorage.getItem("refreshToken");
    if (!refreshToken) return null;
  
    try {
      const res = await fetch("http://localhost:5001/api/auth/refresh-token", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ refreshToken }),
      });
      if (!res.ok) throw new Error("Refresh token failed");
  
      const data = await res.json();
      if (!data.accessToken || !data.refreshToken) {
        throw new Error("Invalid refresh token response");
      }
  
      localStorage.setItem("accessToken", data.accessToken);
      localStorage.setItem("refreshToken", data.refreshToken);
      return data.accessToken;
    } catch (error) {
      console.error("Error refreshing token:", error);
      return null;
    }
  };

  if (!user) {
    return (
      <div>
        <h2>{showRegister ? "Đăng ký" : "Đăng nhập"}</h2>
        {showRegister ? (
          <RegisterForm onRegister={handleRegister} onShowLogin={() => setShowRegister(false)} />
        ) : (
          <LoginForm onLogin={handleLogin} onShowRegister={() => setShowRegister(true)} />
        )}
      </div>
    );
  }

  return (
    <div>
      {editingUser ? (
        <EditUserForm 
          user={user} 
          onUpdateUser={handleUpdateUser} 
          onCancel={() => setEditingUser(false)} 
        />
      ) : (
        <div>
          <span>welcome {user.name} </span>
          <button onClick={() => setEditingUser(true)}>Chỉnh sửa</button>
          <button onClick={handleLogout}>Đăng xuất</button>
        </div>
      )}

      <h1>Tasks</h1>
      <TaskForm
        task={editingTask}
        onCreateTask={handleCreateTask}
        onUpdateTask={handleUpdateTask}
        users={users}
        currentUser={user}
      />
      <ol style={{ listStyleType: "none" }}>
        {tasks.map((task) => (
          <li key={task.id}>
            <strong>Title:</strong> {task.title} <br />
            <strong>Description:</strong> {task.description || "No description"} <br />
            <strong>Status:</strong> {task.status} <br />
            <strong>Due Date:</strong> {task.dueDate ? task.dueDate.split("T")[0] : "N/A"} <br />
            <strong>Assigned To:</strong> {users.find((user) => user.id === task.assignedToId)?.name || "Unassigned"}
            <button onClick={() => setEditingTask(task)}>Sửa</button>
            <button onClick={() => handleDeleteTask(task.id)}>Xóa</button>
          </li>
        ))}
      </ol>
    </div>
  );
};

const LoginForm = ({ onLogin, onShowRegister }) => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const handleSubmit = (e) => {
    e.preventDefault();
    onLogin(email, password);
  };

  return (
    <form onSubmit={handleSubmit}>
      <label>Email:</label>
      <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
      <label>Password:</label>
      <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
      <button type="submit">Đăng nhập</button>
      <p>
        Chưa có tài khoản? <button type="button" onClick={onShowRegister}>Đăng ký</button>
      </p>
    </form>
  );
};

const RegisterForm = ({ onRegister, onShowLogin }) => {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const handleSubmit = (e) => {
    e.preventDefault();
    onRegister(name, email, password);
  };

  return (
    <form onSubmit={handleSubmit}>
      <label>Tên:</label>
      <input type="text" value={name} onChange={(e) => setName(e.target.value)} required />
      <label>Email:</label>
      <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
      <label>Password:</label>
      <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
      <button type="submit">Đăng ký</button>
      <p>
        Đã có tài khoản? <button type="button" onClick={onShowLogin}>Đăng nhập</button>
      </p>
    </form>
  );
};

const EditUserForm = ({ user, onUpdateUser, onCancel }) => {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState(""); 

  useEffect(() => {
    if (user) {
      setName(user.name || "");
      setEmail(user.email || "");
    }
  }, [user]);

  const handleSubmit = (e) => {
    e.preventDefault();
    onUpdateUser({ id: user.id, name, email, password });
  };

  return (
    <form onSubmit={handleSubmit}>
      <label>Tên:</label>
      <input type="text" value={name} onChange={(e) => setName(e.target.value)} required />
      <label>Email:</label>
      <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
      <label>Mật khẩu (để trống nếu không đổi):</label>
      <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
      <button type="submit">Lưu</button>
      <button type="button" onClick={onCancel}>Hủy</button>
    </form>
  );
};
export default App;
