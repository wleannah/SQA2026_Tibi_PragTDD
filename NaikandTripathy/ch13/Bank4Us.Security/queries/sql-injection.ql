/**
 * @name SQL query built from user-controlled sources
 * @description Building a SQL query from user-controlled sources is vulnerable
 *              to insertion of malicious SQL code by the user.
 * @kind path-problem
 * @id cs/sql-injection
 * @problem.severity error
 * @security-severity 8.8
 * @precision high
 * @tags security
 *       external/cwe/cwe-089
 */

import csharp
import semmle.code.csharp.security.dataflow.SqlInjectionQuery
import SqlInjection::PathGraph

from SqlInjection::PathNode source, SqlInjection::PathNode sink
where SqlInjection::flowPath(source, sink)
select sink.getNode(), source, sink,
  "SQL injection: user-controlled input from $@ flows unsanitized to SQL sink.",
  source.getNode(), "this HTTP query-string value"
