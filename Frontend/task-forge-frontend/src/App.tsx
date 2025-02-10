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
        <JobDetails jobId={9} />
      </div>
    </>
  );
}

export default App;
