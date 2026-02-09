grammar Thrift;

document
  : header* definition* EOF
  ;

header
    : namespace
    | include
    ;

definition
  : struct
  | exceptionStruct
  | service
  | typedef
  | union
  | enum
  | DOC_COMMENT
  ;

namespace
  : 'namespace' ID path
  ;

include
  : 'include' DOUBLE_QUOTE_STRING ('as' ID)?
  ;

struct
  : DOC_COMMENT? 'struct' ID LEFT_CURLY_BRACE fields? RIGHT_CURLY_BRACE (LEFT_PARENTHESIS attributes RIGHT_PARENTHESIS)?
  ;

exceptionStruct
  : DOC_COMMENT? 'exception' ID LEFT_CURLY_BRACE fields? RIGHT_CURLY_BRACE (LEFT_PARENTHESIS attributes RIGHT_PARENTHESIS)?
  ;

fields
  : field (COMMA field)*
  ;

field
  : DOC_COMMENT? (UNSIGNED_NUMBER ':')? visibility? type ID (LEFT_PARENTHESIS attributes RIGHT_PARENTHESIS)?
  ;

service
  : DOC_COMMENT? 'service' ID LEFT_CURLY_BRACE functions? RIGHT_CURLY_BRACE (LEFT_PARENTHESIS attributes RIGHT_PARENTHESIS)?
  ;

functions
  : function (COMMA function)*
  ;

throwsFields
  : 'throws' LEFT_PARENTHESIS fields? RIGHT_PARENTHESIS
  ;

function
  : DOC_COMMENT? type ID LEFT_PARENTHESIS fields? RIGHT_PARENTHESIS throwsFields? (LEFT_PARENTHESIS attributes RIGHT_PARENTHESIS)?
  ;

typedef
  : DOC_COMMENT? 'typedef' type ID (LEFT_PARENTHESIS attributes RIGHT_PARENTHESIS)?
  ;

union
  : DOC_COMMENT? 'union' ID LEFT_CURLY_BRACE fields? RIGHT_CURLY_BRACE (LEFT_PARENTHESIS attributes RIGHT_PARENTHESIS)?
  ;

enum
  : DOC_COMMENT? 'enum' ID LEFT_CURLY_BRACE enumValues? RIGHT_CURLY_BRACE (LEFT_PARENTHESIS attributes RIGHT_PARENTHESIS)?
  ;

enumValues
  : enumValue (COMMA enumValue)*
  ;

enumValue
  : DOC_COMMENT? ID ('=' number)?
  ;

type
  : BASE_TYPE
  | VOID_CONSTANT
  | list
  | map
  | ID
  | path
  ;

attributes
  : attribute (COMMA attribute)*
  ;

attribute
  : path '=' value
  ;

value
  : number
  | BOOLEAN_CONSTANT
  | DOUBLE_QUOTE_STRING
  ;

list
  : 'list' LESS_THEN type GRATER_THEN
  ;

map
  : 'map' LESS_THEN type COMMA type GRATER_THEN
  ;

path
  : ID (DOT ID)*
  ;

number
  : SIGNED_NUMBER
  | UNSIGNED_NUMBER
  ;

visibility
  : 'required'
  | 'optional'
  ;

DOC_COMMENT
  : '/**' .*? ('*/' | EOF)
  ;

BASE_TYPE
  : 'bool' | 'byte' | 'i8' | 'i16' | 'i32' | 'i64' | 'double' | 'string' | 'binary' | 'uuid'
  ;

BOOLEAN_CONSTANT
  : 'true' | 'false'
  ;

VOID_CONSTANT
  : 'void'
  ;

UNSIGNED_NUMBER
  : [0-9]+ ('.' [0-9]+)?
  ;

SIGNED_NUMBER
  : '-' [0-9]+ ('.' [0-9]+)?
  ;

ID
  : ( [A-Za-z0-9] | '_' )+
  ;


DOUBLE_QUOTE_STRING
  : '"' (~('"' | '\\') | '\\' . )* '"'
  ;

LEFT_PARENTHESIS
  : '('
  ;

RIGHT_PARENTHESIS
  : ')'
  ;

LEFT_CURLY_BRACE
  : '{'
  ;

RIGHT_CURLY_BRACE
  : '}'
  ;

LESS_THEN
  : '<'
  ;

COMMA
  : ','
  ;

DOT
  : '.'
  ;

GRATER_THEN
  : '>'
  ;

COMMENT
  :  '#' ~[\r\n]* -> skip
  ;

SPACE
  : [ \t\r\n]+ -> skip
  ;
