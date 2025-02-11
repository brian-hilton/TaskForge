import { useState } from "react";

import "./App.css";
import JobDetails from "./components/JobDetails";

function App() {
  return (
    <>
      <div>
        <h1>Job Viewer</h1>
        <JobDetails jobId={1} />
        <JobDetails jobId={2} />
      </div>
    </>
  );
}

export default App;
