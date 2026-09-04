--
-- PostgreSQL database dump
--

\restrict tvHyLNkaDzKn74QtixTVmvYAMzyZoaV6qOsIUg0oqtEt9GQizVdICk5WDfrL2D0

-- Dumped from database version 17.6
-- Dumped by pg_dump version 17.6

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Data for Name: GzNutritionOrder; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."GzNutritionOrder" ("Id", "OrderNumber", "PatientId", "EncounterId", "RequesterDoctorId", "AssignedWorkforceId", "Status", "Priority", "ReasonForReferral", "ScreeningRiskStatus", "ScreeningScore", "RequestedAt", "ClosedAt", "ClosingNote", "Version", "CreateDateTime", "CreateBy", "UpdateDateTime", "UpdateBy", "DeleteDateTime", "DeleteBy", "CancelDateTime", "CancelBy", "IsCancel", "IsDelete") VALUES ('3a82f0cb-0afa-591e-e330-b5bfd4613c95', 'GZ-3a82f0cb0afa591ee', '92a1f69c-6e2a-453d-bc72-5441748873c6', '91ea577c-1bb8-4b9c-8aaf-ef2ef4ebfc59', 'a2f85c74-ef0f-40c6-aaf4-920019d7322d', '05d2a963-fd58-465b-87c8-7209d0806c42', 1, 1, 'karna kurang sehat', NULL, NULL, '2026-09-01 13:43:34.127242+07', NULL, NULL, 0, '2026-09-01 13:43:34.127242+07', '0ba84a1a-2559-49ba-a320-10fb1f399d70', NULL, '00000000-0000-0000-0000-000000000000', NULL, '00000000-0000-0000-0000-000000000000', NULL, '00000000-0000-0000-0000-000000000000', false, false);


--
-- Data for Name: GzNutritionCareRecord; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- Data for Name: GzNutritionOrderHistory; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."GzNutritionOrderHistory" ("Id", "NutritionOrderId", "FromStatus", "ToStatus", "Action", "Reason", "ActorUserId", "OccurredAt", "Source", "CorrelationId", "CreateDateTime", "CreateBy", "UpdateDateTime", "UpdateBy", "DeleteDateTime", "DeleteBy", "CancelDateTime", "CancelBy", "IsCancel", "IsDelete") VALUES ('4cf3941b-b231-4b7b-b64d-67ae30a6e136', '3a82f0cb-0afa-591e-e330-b5bfd4613c95', NULL, 1, 'CreateOrder', NULL, '0ba84a1a-2559-49ba-a320-10fb1f399d70', '2026-09-01 13:43:34.127242+07', 'API:A6F54EA104B4237ACAFD0C55207EF3241F5DFE32D00F62', 'gz-order-1788245014030-b4q7qcg7', '2026-09-01 13:43:34.127242+07', '0ba84a1a-2559-49ba-a320-10fb1f399d70', NULL, '00000000-0000-0000-0000-000000000000', NULL, '00000000-0000-0000-0000-000000000000', NULL, '00000000-0000-0000-0000-000000000000', false, false);


--
-- PostgreSQL database dump complete
--

\unrestrict tvHyLNkaDzKn74QtixTVmvYAMzyZoaV6qOsIUg0oqtEt9GQizVdICk5WDfrL2D0

