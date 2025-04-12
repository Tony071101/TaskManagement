import { useState, useEffect } from "react";

const TaskForm = ({ task, onCreateTask, onUpdateTask, users, currentUser }) => {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [status, setStatus] = useState("To-Do");
  const [dueDate, setDueDate] = useState("");
  const [assignedTo, setAssignedTo] = useState("");

  useEffect(() => {
    if(task) {
      setTitle(task ? task.title : "");
      setDescription(task.description || "");
      setStatus(task.status || "To-Do");
      setDueDate(task.dueDate ? task.dueDate.split("T")[0] : "");
      setAssignedTo(task.assignedToId || "");
    } else {
      setTitle("");
      setDescription("");
      setStatus("To-Do");
      setDueDate("");
      setAssignedTo("");
    }
  }, [task]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!title.trim()) return;

    const taskData = { 
      title, 
      description, 
      status,
      dueDate, 
      assignedToId: assignedTo ? parseInt(assignedTo) : null,
    };

    if (task) {
      await onUpdateTask({ ...task, ...taskData });
    } else {
      await onCreateTask(taskData);
    }

    // Reset form
    setTitle("");
    setDescription("");
    setStatus("To-Do");
    setDueDate("");
    setAssignedTo("");
  };

  const handleStatusChange = (e) => {
    if (currentUser.id === assignedTo) {
      setStatus(e.target.value);  // chỉ người được giao nhiệm vụ mới có thể thay đổi trạng thái
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <input type="text" value={title} onChange={(e) => setTitle(e.target.value)} required />
      <input value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Description" />
      {task && currentUser.id === assignedTo && (
        <select value={status} onChange={handleStatusChange}>
          <option value="In Progress">In Progress</option>
          <option value="Done">Done</option>
        </select>
      )}
      <input type="date" value={dueDate} onChange={(e) => setDueDate(e.target.value)} required/>
      <select value={assignedTo} onChange={(e) => setAssignedTo(e.target.value)} required>
        <option value="">Chọn người phụ trách</option>
        {users.map((user) => (
          <option key={user.id} value={user.id}>
            {user.name}
          </option>
        ))}
      </select>
      <button type="submit">{task ? "Cập nhật" : "Thêm"}</button>
      {task && <button type="button" onClick={() => onUpdateTask(null)}>Hủy</button>}
    </form>
  );
};

export default TaskForm;
