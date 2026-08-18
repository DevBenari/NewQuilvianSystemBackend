# Laporan Setup AI Skills Pengembangan Modul

| Field | Nilai |
| --- | --- |
| Tanggal | 2026-08-13 |
| Branch backend | `MHamzah` |
| Branch frontend | `HamzahV2` |
| Status | Selesai untuk tahap setup project |
| Scope | Konfigurasi AI agent dan dokumentasi |
| Source aplikasi | Tidak diubah |
| Endpoint/DTO/model/service | Tidak ada perubahan |
| Database/runtime migration | Tidak ada |
| Build aplikasi | Tidak dijalankan |

## Tujuan

Memasang suite AI skill yang didefinisikan oleh
`docs/agency/rekomendasi-ai-skills-pengembangan-modul.md` pada project backend dan frontend,
dengan satu prosedur canonical untuk shared workflow dan tanpa mengubah backup
`agency-local/`.

## Hasil implementasi

### Canonical package

Enam package ditempatkan di backend:

```text
QuilvianBackend/.agents/skills/
  grill-me/
  trace-existing-capabilities/
  design-business-module/
  plan-module-delivery/
  verify-module-readiness/
  build-module-backend/
```

Satu package implementation ditempatkan di frontend:

```text
QuilvianFrontEnd/.agents/skills/build-module-frontend/
```

Setiap canonical package memiliki `SKILL.md`, `agents/openai.yaml`, dan reference hanya jika
workflow membutuhkannya. Pembagian konseptual tetap 5 shared + 2 repo-local, sedangkan
custody canonical adalah 6 backend + 1 frontend.

### Adapter shared frontend

Lima adapter dipasang pada:

```text
QuilvianFrontEnd/.agents/skills/
  grill-me/
  trace-existing-capabilities/
  design-business-module/
  plan-module-delivery/
  verify-module-readiness/
```

Adapter tidak menyalin prosedur. Masing-masing menunjuk sibling canonical backend dan
memeriksa SHA-256 `SKILL.md`. Perbedaan hash menghasilkan kondisi
`STALE_SHARED_SKILL`, sehingga drift tidak diterima secara diam-diam.

### Blueprint dan handoff

Suite menghasilkan satu sumber artefak pada
`QuilvianBackend/docs/module-blueprints/<module>/`. Urutan handoff-nya:

```text
grill-me
  -> trace-existing-capabilities
  -> design-business-module
  -> plan-module-delivery
  -> build-module-backend / build-module-frontend
  -> verify-module-readiness
```

Builder backend/frontend hanya mengerjakan satu task approved. Shared skill membawa
guard untuk decision status, contract version, input revision/hash, source SHA, authority
UI, reuse tabel existing, dan traceability.

## File material

| Lokasi | Perubahan |
| --- | --- |
| `QuilvianBackend/.agents/skills/` | Enam canonical package dan references |
| `QuilvianFrontEnd/.agents/skills/` | Satu canonical frontend builder dan lima adapter shared |
| `docs/agency/rekomendasi-ai-skills-pengembangan-modul.md` | Diselaraskan dari rekomendasi menjadi setup aktual |
| `docs/hamzah/report/setup-ai-skills-pengembangan-modul.md` | Laporan ini |

Perubahan `.gitignore`, penghapusan nama dokumen lama, dan isi `agency-local/` yang terlihat
pada worktree sudah ada sebelum implementasi ini. Perubahan tersebut tidak diubah atau
dipulihkan oleh agent.

## Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| `quick_validate.py` | Lulus 12/12 package: 7 canonical + 5 adapter |
| Placeholder/TODO scan | Lulus; tidak ada marker tersisa |
| Package inventory | Lulus; semua memiliki `SKILL.md` dan `agents/openai.yaml` |
| Batas ukuran `SKILL.md` | Lulus; seluruh file di bawah 500 baris |
| Hash adapter | Lulus; 5/5 SHA-256 cocok dengan canonical backend |
| Git ignore | Lulus; sample backend, frontend builder, dan frontend adapter trackable |
| Pembagian ownership | Lulus; 6 backend + 1 frontend canonical, 5 frontend adapter |
| Build/test aplikasi | Tidak dijalankan karena source aplikasi tidak berubah |
| Forward-test IGD/non-IGD | Belum dijalankan; menjadi tahap validasi perilaku berikutnya |

## Catatan risiko dan operasi

- Bila isi salah satu shared canonical `SKILL.md` berubah, hash adapter terkait wajib
  diperbarui dalam perubahan yang sama.
- Lima shared skill belum dipromosikan ke user-global; lakukan forward-test minimal pada
  IGD dan pengkajian rawat inap terlebih dahulu.
- Sesi frontend membutuhkan sibling `QuilvianBackend` pada workspace yang sama untuk
  menjalankan shared canonical. Adapter akan berhenti terkendali bila sibling tidak tersedia.
- Git backend tetap read-only bagi agent; tidak ada `git add`, commit, push, pull, atau merge
  yang dilakukan.

## Rekomendasi tahap berikut

1. Commit package backend dan frontend pada branch masing-masing oleh developer.
2. Restart/reopen agent dari masing-masing repository agar discovery `.agents/skills/`
   membaca package baru.
3. Forward-test `grill-me` sampai `verify-module-readiness` pada satu skenario IGD dan satu
   skenario pengkajian rawat inap.
4. Setelah lolos, pertimbangkan memindahkan lima shared skill ke repository engineering
   tersendiri atau package global yang memiliki versioning formal.
