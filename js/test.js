async function fetchStudentData(className, sectionName, accordionId, sortByNameId, sortByTotalId, sortOptions) {
    try {
      const response = await fetch("utils/students.json");
      if (!response.ok) {
        throw new Error(`Failed to fetch: ${response.statusText}`);
      }
  
      const students = await response.json();
  
      // Filter by class and section
      const filteredStudents = students.filter(
        (student) => student.class === className && student.section === sectionName
      );
  
      const accordion = document.getElementById(accordionId);
  
      // Function to generate the table in the specified format
      function createReportCardTable(student) {
        const table = document.createElement("table");
        table.classList.add("table", "table-bordered", "table-responsive");
  
        // Table header
        const headerRow = document.createElement("tr");
        headerRow.innerHTML = `
          <th>Scholastic Area</th>
          <th colspan="6">Term I</th>
          <th colspan="6">Term II</th>
        `;
  
        const subHeaderRow = document.createElement("tr");
        subHeaderRow.innerHTML = `
          <td>Subject</td>
          <td>Written Test (10)</td>
          <td>Subject Enrichment (5)</td>
          <td>CW & HW (5)</td>
          <td>Half Yearly Exam (80)</td>
          <td>Marks Obtained (100)</td>
          <td>GRADE</td>
          <td>Written Test (10)</td>
          <td>Subject Enrichment (5)</td>
          <td>CW & HW (5)</td>
          <td>Annual Exam (80)</td>
          <td>Marks Obtained (100)</td>
          <td>GRADE</td>
        `;
  
        table.append(headerRow, subHeaderRow);
  
        // Populate the table with subjects and corresponding marks/grades for each term
        const subjects = ["English", "Maths", "Science", "Social Studies", "Telugu", "Hindi"];
  
        subjects.forEach((subject) => {
          const term1Data = student.grades[subject]?.term1 || {};
          const term2Data = student.grades[subject]?.term2 || {};
  
          const row = document.createElement("tr");
          row.innerHTML = `
            <td>${subject}</td>
            <td>${term1Data.writtenTest ?? ""}</td>
            <td>${term1Data.enrichment ?? ""}</td>
            <td>${term1Data.cwHw ?? ""}</td>
            <td>${term1Data.halfYearly ?? ""}</td>
            <td>${term1Data.total ?? ""}</td>
            <td>${term1Data.grade ?? ""}</td>
            <td>${term2Data.writtenTest ?? ""}</td>
            <td>${term2Data.enrichment ?? ""}</td>
            <td>${term2Data.cwHw ?? ""}</td>
            <td>${term2Data.annual ?? ""}</td>
            <td>${term2Data.total ?? ""}</td>
            <td>${term2Data.grade ?? ""}</td>
          `;
  
          table.append(row);
        });
  
        // Total row with term-wise totals
        const totalRow = document.createElement("tr");
        totalRow.innerHTML = `
          <td>Total (600)</td>
          <td colspan="4">TERM - I (TOTAL)</td>
          <td colspan="2">${student.termTotals.term1}</td>
          <td colspan="4">TERM - II (TOTAL)</td>
          <td colspan="2">${student.termTotals.term2}</td>
        `;
  
        table.append(totalRow);
  
        return table;
      }
  
      // Function to render the accordion content
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
                  <div class="row d-flex flex-wrap justify-content-between align-items-center">
                    <div class="col-12 col-lg-2">
                      <strong style="color:#1D3557;">${student.name}</strong>
                    </div>
                    <div class="col-12 col-lg-8 d-flex flex-wrap justify-content-between">
                      ${Object.entries(student.grades)
                        .map(([subject, grade]) => `<div><i class="px-2">${subject}</i> <strong style="color:#1D3557;">${grade.term1.total}</strong>`)
                        .join("")}
                    </div>
                    <div class="col-12 col-lg-2 text-end">
                      <strong style="color:#E63946;">Total: Term 1 - ${student.termTotals.term1}, Term 2 - ${student.termTotals.term2}</strong>
                    </div>
                  </div>
                </div>
              </div>
            </a>
          `;
  
          const reportCardTable = createReportCardTable(student);
  
          const cardBody = `
            <div id="${studentID}" class="collapse" data-bs-parent="#${accordionId}">
              <div class="card-body">
                <strong>Roll No:</strong> ${student.rollNo}<br>
                <strong>Class:</strong> ${student.class}<br>
                <strong>Section:</strong> ${student.section}<br>
                <strong>Guardian:</strong> ${student.guardian}<br>
              </div>
            </div>
          `;
  
          studentCard.innerHTML = cardHeader + cardBody;
  
          const cardBodyElement = studentCard.querySelector(".card-body");
          cardBodyElement.appendChild(reportCardTable); // Add the generated table to the card body
  
          accordion.appendChild(studentCard);
        });
      }
  
      renderAccordion(filteredStudents); // Render with filtered data
  
      // Event listeners for sorting
      document.getElementById(sortByNameId).addEventListener("click", () => {
        const sortedByName = [...filteredStudents].sort((a, b) => a.name.localeCompare(b.name));
        renderAccordion(sortedByName); // Re-render with sorted data
      });
  
      document.getElementById(sortByTotalId).addEventListener("click", () => {
        const sortedByTotal = [...filteredStudents].sort((a, b) => b.termTotals.term2 - a.termTotals.term1);
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
  