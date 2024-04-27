async function fetchStudentData(className, sectionName, accordionId, sortByNameId, sortByTotalId) {
  try {
    const response = await fetch("utils/students.json");
    if (!response.ok) {
      throw new Error(`Failed to fetch: ${response.statusText}`);
    }

    const students = await response.json();

    // Filter by class and section
    const filteredStudents = students.filter(student =>
      student.class === className && student.section === sectionName
    );

    const accordion = document.getElementById(accordionId);

    // Function to render the filtered and sorted students in the accordion
    function renderAccordion(data) {
      accordion.innerHTML = ""; // Clear existing content
      data.forEach((student, index) => {
        const studentCard = document.createElement("div");
        studentCard.classList.add("card", "mb-2");

        const studentID = `${accordionId}Student${index + 1}`; // Unique ID

        const cardHeader = `
<a data-bs-toggle="collapse" href="#${studentID}">
  <div class="card-header">
    <div class="container-fluid">
      <!-- Row with flexbox to ensure elements adjust automatically -->
      <div class="row d-flex flex-wrap justify-content-between align-items-center">
        <!-- Student Name -->
        <div class="col-12 col-lg-2">
          <strong style="color:#1D3557;">${student.name}</strong>
        </div>

        <!-- Subject Grades -->
        <div class="col-12 col-lg-8 d-flex flex-wrap justify-content-between">
          ${Object.entries(student.grades)
            .map(([subject, grade]) => `<div><i class="px-2">${subject}</i> <strong
              style="color:#1D3557;">${grade}</strong></div>`)
            .join("")}
        </div>

        <!-- Total -->
        <div class="col-12 col-lg-2 text-end">
          <strong style="color:#E63946;">Total ${student.total}</strong>
        </div>
      </div>
    </div>
  </div>
</a>
`;

        const cardBody = `
<div id="${studentID}" class="collapse" data-bs-parent="#${accordionId}">
  <div class="card-body">
    <strong>Roll No:</strong> ${student.rollNo}<br>
    <strong>Class:</strong> ${student.class}<br>
    <strong>Section:</strong> ${student.section}<br>
    <strong>Guardian:</strong> ${student.guardian}
  </div>
</div>
`;

        studentCard.innerHTML = cardHeader + cardBody;

        accordion.appendChild(studentCard);
      });
    }

    renderAccordion(filteredStudents); // Render filtered data

    // Event listeners for sorting
    document.getElementById(sortByNameId).addEventListener("click", () => {
      const sortedByName = [...filteredStudents].sort((a, b) => a.name.localeCompare(b.name));
      renderAccordion(sortedByName); // Re-render with sorted data
    });

    document.getElementById(sortByTotalId).addEventListener("click", () => {
      const sortedByTotal = [...filteredStudents].sort((a, b) => b.total - a.total);
      renderAccordion(sortedByTotal); // Re-render with sorted data
    });

  } catch (error) {
    console.error("Error fetching student data:", error);
  }
}

// Event listeners for tab switching
document.getElementById("sectiona-tab").addEventListener("shown.bs.tab", () => {
  fetchStudentData(9, "A", "accordionA", "sortByNameA", "sortByTotalA");
});

document.getElementById("sectionb-tab").addEventListener("shown.bs.tab", () => {
  fetchStudentData(9, "B", "accordionB", "sortByNameB", "sortByTotalB");
});

// Set a default tab to fetch data when the page loads
fetchStudentData(9, "B", "accordionB", "sortByNameB", "sortByTotalB");