using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class NormalizeHrdAndMstTablesToQuilvianStandard : Migration
    {
        // QBE-NAM-001, QBE-NAM-002, QBE-NAM-003, QBE-DB-002
        // Normalisasi 28 tabel Human Resource dan Performance Management ke standar Quilvian:
        // - Mengganti format snake_case jamak (hrd_*, mst_*, trx_*) menjadi PascalCase tunggal
        // - Mengganti prefix terlarang trx_* menjadi prefix resmi Hrd
        // - Menjaga keutuhan data fisik (non-destruktif RENAME TABLE tanpa DROP+CREATE)

        private const string Peta = @"
                'hrd_clinical_peer_review_audit_logs', 'HrdClinicalPeerReviewAuditLog',
                'hrd_training_bond_contracts', 'HrdTrainingBondContract',
                'mst_kpi_metric_catalogs', 'MstKpiMetricCatalog',
                'mst_oppe_metric_catalogs', 'MstOppeMetricCatalog',
                'trx_bipartite_negotiation_forums', 'HrdBipartiteNegotiationForum',
                'trx_clinical_career_ladder_reviews', 'HrdClinicalCareerLadderReview',
                'trx_disciplinary_incident_reports', 'HrdDisciplinaryIncidentReport',
                'trx_disciplinary_investigation_records', 'HrdDisciplinaryInvestigationRecord',
                'trx_disciplinary_sanctions', 'HrdDisciplinarySanction',
                'trx_disciplinary_suspensions', 'HrdDisciplinarySuspension',
                'trx_exit_clearance_checklists', 'HrdExitClearanceChecklist',
                'trx_exit_clearance_processes', 'HrdExitClearanceProcess',
                'trx_fppe_case_logs', 'HrdFppeCaseLog',
                'trx_fppe_engagements', 'HrdFppeEngagement',
                'trx_fppe_evaluation_conclusions', 'HrdFppeEvaluationConclusion',
                'trx_oppe_evaluation_periods', 'HrdOppeEvaluationPeriod',
                'trx_oppe_staff_evaluations', 'HrdOppeStaffEvaluation',
                'trx_oppe_staff_evaluation_details', 'HrdOppeStaffEvaluationDetail',
                'trx_performance_appraisals', 'HrdPerformanceAppraisal',
                'trx_performance_appraisal_details', 'HrdPerformanceAppraisalDetail',
                'trx_performance_appraisal_disputes', 'HrdPerformanceAppraisalDispute',
                'trx_performance_calibration_sessions', 'HrdPerformanceCalibrationSession',
                'trx_performance_cycles', 'HrdPerformanceCycle',
                'trx_performance_goals', 'HrdPerformanceGoal',
                'trx_performance_improvement_plans', 'HrdPerformanceImprovementPlan',
                'trx_performance_pip_action_logs', 'HrdPerformancePipActionLog',
                'trx_plataran_sehat_skp_records', 'HrdPlataranSehatSkpRecord',
                'trx_severance_calculations', 'HrdSeveranceCalculation'";

        private const string PetaBalik = @"
                'HrdClinicalPeerReviewAuditLog', 'hrd_clinical_peer_review_audit_logs',
                'HrdTrainingBondContract', 'hrd_training_bond_contracts',
                'MstKpiMetricCatalog', 'mst_kpi_metric_catalogs',
                'MstOppeMetricCatalog', 'mst_oppe_metric_catalogs',
                'HrdBipartiteNegotiationForum', 'trx_bipartite_negotiation_forums',
                'HrdClinicalCareerLadderReview', 'trx_clinical_career_ladder_reviews',
                'HrdDisciplinaryIncidentReport', 'trx_disciplinary_incident_reports',
                'HrdDisciplinaryInvestigationRecord', 'trx_disciplinary_investigation_records',
                'HrdDisciplinarySanction', 'trx_disciplinary_sanctions',
                'HrdDisciplinarySuspension', 'trx_disciplinary_suspensions',
                'HrdExitClearanceChecklist', 'trx_exit_clearance_checklists',
                'HrdExitClearanceProcess', 'trx_exit_clearance_processes',
                'HrdFppeCaseLog', 'trx_fppe_case_logs',
                'HrdFppeEngagement', 'trx_fppe_engagements',
                'HrdFppeEvaluationConclusion', 'trx_fppe_evaluation_conclusions',
                'HrdOppeEvaluationPeriod', 'trx_oppe_evaluation_periods',
                'HrdOppeStaffEvaluation', 'trx_oppe_staff_evaluations',
                'HrdOppeStaffEvaluationDetail', 'trx_oppe_staff_evaluation_details',
                'HrdPerformanceAppraisal', 'trx_performance_appraisals',
                'HrdPerformanceAppraisalDetail', 'trx_performance_appraisal_details',
                'HrdPerformanceAppraisalDispute', 'trx_performance_appraisal_disputes',
                'HrdPerformanceCalibrationSession', 'trx_performance_calibration_sessions',
                'HrdPerformanceCycle', 'trx_performance_cycles',
                'HrdPerformanceGoal', 'trx_performance_goals',
                'HrdPerformanceImprovementPlan', 'trx_performance_improvement_plans',
                'HrdPerformancePipActionLog', 'trx_performance_pip_action_logs',
                'HrdPlataranSehatSkpRecord', 'trx_plataran_sehat_skp_records',
                'HrdSeveranceCalculation', 'trx_severance_calculations'";

        private static string Skrip(string peta) => $$"""
            DO $qbe$
            DECLARE
                peta CONSTANT text[] := ARRAY[{{peta}}
                ];
                lama text;
                baru text;
                i int;
                r record;
            BEGIN
                -- 0. Pemeriksaan tabrakan.
                FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
                    lama := peta[i * 2 - 1];
                    baru := peta[i * 2];
                    IF EXISTS (
                        SELECT 1 FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE n.nspname = 'public' AND c.relname = lama AND c.relkind = 'r'
                    ) AND EXISTS (
                        SELECT 1 FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE n.nspname = 'public' AND c.relname = baru AND c.relkind = 'r'
                    ) THEN
                        RAISE EXCEPTION 'Tabel % dan % sama-sama ada di skema public; rename dibatalkan agar tidak menimpa data.', lama, baru;
                    END IF;
                END LOOP;

                -- 1. Nama tabel.
                FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
                    lama := peta[i * 2 - 1];
                    baru := peta[i * 2];
                    IF EXISTS (
                        SELECT 1 FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE n.nspname = 'public' AND c.relname = lama AND c.relkind = 'r'
                    ) THEN
                        EXECUTE format('ALTER TABLE public.%I RENAME TO %I', lama, baru);
                    END IF;
                END LOOP;

                -- 2. Constraint (PK, Unique, FK, Check).
                FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
                    lama := peta[i * 2 - 1];
                    baru := peta[i * 2];
                    FOR r IN
                        SELECT t.relname AS tabel, c.conname AS nama
                        FROM pg_constraint c
                        JOIN pg_class t ON t.oid = c.conrelid
                        JOIN pg_namespace n ON n.oid = t.relnamespace
                        WHERE n.nspname = 'public'
                          AND c.conname ~ ('(^|_)' || lama || '(_|~|$)')
                    LOOP
                        EXECUTE format(
                            'ALTER TABLE public.%I RENAME CONSTRAINT %I TO %I',
                            r.tabel, r.nama,
                            regexp_replace(r.nama, '(^|_)' || lama || '(?=_|~|$)', '\1' || baru, 'g'));
                    END LOOP;
                END LOOP;

                -- 3. Sisa index yang tidak ditopang constraint.
                FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
                    lama := peta[i * 2 - 1];
                    baru := peta[i * 2];
                    FOR r IN
                        SELECT c.relname AS nama
                        FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE n.nspname = 'public' AND c.relkind = 'i'
                          AND c.relname ~ ('(^|_)' || lama || '(_|~|$)')
                    LOOP
                        EXECUTE format(
                            'ALTER INDEX public.%I RENAME TO %I',
                            r.nama,
                            regexp_replace(r.nama, '(^|_)' || lama || '(?=_|~|$)', '\1' || baru, 'g'));
                    END LOOP;
                END LOOP;
            END
            $qbe$;
            """;

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(Skrip(Peta));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(Skrip(PetaBalik));
        }
    }
}
